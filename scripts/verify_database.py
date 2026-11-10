#!/usr/bin/env python3
"""Verificação somente de leitura do SQLite do Starlink System."""
import argparse
import sqlite3
import sys
from pathlib import Path


EXPECTED_TABLES = {"Donos", "Clientes", "Vendas", "__EFMigrationsHistory"}
EXPECTED_TRIGGERS = {
    "TR_Vendas_ValidarInsert",
    "TR_Vendas_ValidarUpdate",
    "TR_Clientes_BloquearTrocaDeDono",
}
EXPECTED_MIGRATIONS = {
    "20260901143358_InitialCreate",
    "20260901160137_AddAutenticacaoDono",
    "20261110203000_AddBancoIntegrityGuards",
}


def verify(path: Path) -> int:
    if not path.is_file():
        print(f"ERRO: banco não encontrado: {path}", file=sys.stderr)
        return 1

    # mode=ro impede que a inspeção modifique o banco.
    with sqlite3.connect(path.resolve().as_uri() + "?mode=ro", uri=True) as db:
        db.execute("PRAGMA foreign_keys = ON")
        integrity = db.execute("PRAGMA integrity_check").fetchone()[0]
        fk_errors = db.execute("PRAGMA foreign_key_check").fetchall()
        tables = {
            row[0] for row in db.execute(
                "SELECT name FROM sqlite_master WHERE type = 'table'"
            )
        }
        triggers = {
            row[0] for row in db.execute(
                "SELECT name FROM sqlite_master WHERE type = 'trigger'"
            )
        }

        missing_tables = EXPECTED_TABLES - tables
        missing_triggers = EXPECTED_TRIGGERS - triggers
        if missing_tables or missing_triggers:
            print(f"ERRO: tabelas ausentes: {sorted(missing_tables)}")
            print(f"ERRO: gatilhos ausentes: {sorted(missing_triggers)}")
            return 1

        migrations = {
            row[0] for row in db.execute(
                'SELECT "MigrationId" FROM "__EFMigrationsHistory"'
            )
        }
        missing_migrations = EXPECTED_MIGRATIONS - migrations
        owner_mismatch = db.execute("""
            SELECT COUNT(*) FROM "Vendas" AS v
            LEFT JOIN "Clientes" AS c ON c."Id" = v."ClienteId"
            WHERE c."Id" IS NULL OR c."DonoId" <> v."DonoId"
        """).fetchone()[0]
        invalid_values = db.execute("""
            SELECT COUNT(*) FROM "Vendas"
            WHERE "Valor" <= 0
               OR ("DataFimAluguel" IS NOT NULL
                   AND "DataFimAluguel" < "DataInicioAluguel")
        """).fetchone()[0]

        print(f"Arquivo: {path}")
        print(f"Integridade SQLite: {integrity}")
        print(f"Chaves estrangeiras inconsistentes: {len(fk_errors)}")
        print(f"Vendas com dono divergente: {owner_mismatch}")
        print(f"Vendas com valor ou período inválido: {invalid_values}")
        print(f"Migrations instaladas: {len(migrations)}")

        for table in ("Donos", "Clientes", "Vendas"):
            count = db.execute(f'SELECT COUNT(*) FROM "{table}"').fetchone()[0]
            print(f"{table}: {count} registro(s)")

        if integrity != "ok" or fk_errors or missing_migrations or owner_mismatch or invalid_values:
            print(f"FALHA. Migrations pendentes: {sorted(missing_migrations)}")
            return 1

        print("OK: estrutura, migrations e integridade verificadas.")
        return 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    default_db = Path(__file__).resolve().parents[1] / "Backend" / "starlink.db"
    parser.add_argument("database", nargs="?", type=Path, default=default_db)
    args = parser.parse_args()
    sys.exit(verify(args.database))
