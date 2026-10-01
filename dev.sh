#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "${BASH_SOURCE[0]}")"

if [[ ! -f .env ]]; then
  echo "Missing .env in the solution directory." >&2
  exit 1
fi

# This is the private Bash-formatted .env file you created earlier.
set -a
source ./.env
set +a

if [[ -z "${CREDIT_SQL_PASSWORD:-}" ]]; then
  echo "CREDIT_SQL_PASSWORD is missing from .env." >&2
  exit 1
fi

if ! docker info >/dev/null 2>&1; then
  echo "Docker is not running or is not accessible." >&2
  exit 1
fi

running="$(docker inspect --format '{{.State.Running}}' credit-sql 2>/dev/null)" || {
  echo "The credit-sql container was not found." >&2
  exit 1
}

if [[ "$running" != "true" ]]; then
  docker start credit-sql >/dev/null
fi

case "${1:-web}" in
web)
  exec dotnet run --project src/CreditAssessment.Web \
    --urls http://127.0.0.1:5090
  ;;
demo)
  exec dotnet run --project src/CreditAssessment.Demo
  ;;
*)
  echo "Usage: ./dev.sh [web|demo]" >&2
  exit 2
  ;;
esac
