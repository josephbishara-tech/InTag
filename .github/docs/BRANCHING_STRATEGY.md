# InTag — GitFlow Branching Strategy

## Branches

| Branch | Purpose | Deploys To |
|---|---|---|
| `main` | Production-ready code | Production (via manual approval) |
| `develop` | Integration branch for features | — (CI only) |
| `feature/*` | New features | — (CI on PR) |
| `release/*` | Release candidates | Staging |
| `hotfix/*` | Production bug fixes | Staging → Production |

## Workflow

### Feature Development
1. Create `feature/INTAG-123-asset-registry` from `develop`
2. Develop, commit, push
3. Open PR to `develop` — triggers CI (build + test + SonarQube)
4. Code review (minimum 1 approval)
5. Merge to `develop`

### Release
1. Create `release/1.0.0` from `develop`
2. Final QA, bug fixes committed to release branch
3. CI runs → deploys to Staging automatically
4. Integration tests run against Staging
5. When approved: merge to `main` AND back to `develop`
6. Tag `v1.0.0` on `main`
7. Manual trigger of production deploy workflow

### Hotfix
1. Create `hotfix/1.0.1-fix-login` from `main`
2. Fix, push — CI runs, deploys to Staging
3. Verify on Staging
4. Merge to `main` AND `develop`
5. Tag `v1.0.1`, deploy to Production

## Commit Convention
```
type(scope): description

feat(asset): add depreciation calculation engine
fix(auth): resolve token refresh race condition
docs(readme): update setup instructions
chore(deps): bump EF Core to 10.0.1
```

## Code Review Rules
- Minimum 1 approval required for `develop`
- Minimum 2 approvals required for `main`
- All CI checks must pass before merge
- No direct commits to `main` or `develop`