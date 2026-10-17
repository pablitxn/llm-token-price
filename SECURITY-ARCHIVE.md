# Archived project security

The owner confirms this project is fully deprecated. Existing provider credentials, external copies and persistent database volumes are not independently verified retired.

Docker Compose requires a private POSTGRES_PASSWORD. Generate a fresh value locally and provide it through ignored .env or environment configuration. Changing that variable does not rotate an existing pgdata volume; do not delete/reset data to simulate rotation.

The API consumes ConnectionStrings:DefaultConnection (environment key ConnectionStrings__DefaultConnection). Legacy DATABASE_CONNECTION_STRING examples are empty; there is no automatic dotenv mapper in the inspected API. Configure Admin__Username/Admin__Password and a private Jwt__SecretKey/JWT_SECRET_KEY explicitly. Templates intentionally contain no usable password or signing key.

Authentication tests now generate credentials and signing material per in-process factory and inject them through native configuration. Existing successful-login and invalid-login behavior is preserved. The shared E2E handler still bypasses auth for its original non-authentication tests; dedicated infrastructure auth tests retain the real handler. Ambient database/Redis destinations and destructive initialization still require isolated dependencies before full application/E2E execution. Do not run those suites against shared services.

Source/history containment does not revoke credentials or erase GitHub-owned PR refs, old SHA objects, copied artifacts or other clones. Full GitGuardian closure requires those separate checks.
