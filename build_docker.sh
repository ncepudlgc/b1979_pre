#!/bin/bash
NAMESPACE="${1:-codebase_b1979_app}"
docker build -t "$NAMESPACE" .