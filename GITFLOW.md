# Git Branching Strategy — Gitflow

This project follows the **Gitflow** branching model.

---

## Branch Structure

```
master          ← Production-ready code. Always stable. Tagged with versions.
  ├── hotfix/v1.0.1    ← Emergency fix branching off master
  │
develop         ← Integration branch. All features merge here first.
  ├── feature/identity-service
  ├── feature/notification-service
  ├── feature/cart-rename-from-basket
  ├── feature/api-gateway-routes
  ├── feature/docker-orchestration
  └── release/v1.1.0   ← Release stabilization branch
```

---

## Branch Roles

| Branch | Base | Merges Into | Purpose |
|--------|------|-------------|---------|
| `master` | — | — | Production-ready, always deployable |
| `develop` | `master` | `master` (via release) | Main integration branch |
| `feature/*` | `develop` | `develop` | New features, one per feature |
| `release/*` | `develop` | `master` + `develop` | Release stabilization & bug fixes |
| `hotfix/*` | `master` | `master` + `develop` | Critical production bug fixes |

---

## Version Tags (on `master`)

| Tag | Commit | Description |
|-----|--------|-------------|
| `v0.1.0` | `a894ee0` | Initial Cart rename from Basket — baseline microservices |
| `v0.5.0` | `2bea01e` | Complete Basket→Cart refactoring across all services |
| `v0.9.0` | `d47a65e` | Identity and Notification services scaffold |
| `v1.0.0` | `512cc94` | **Production-ready** — Cart, Identity, Notification fully implemented |

---

## Workflows

### 1. Starting a New Feature

```bash
# Always branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# ... do your work ...
git add .
git commit -m "feat(scope): description of feature"

# Push your branch
git push origin feature/your-feature-name
```

> Open a Pull Request from `feature/your-feature-name` → `develop`

---

### 2. Creating a Release

```bash
# Branch from develop when feature-complete
git checkout develop
git pull origin develop
git checkout -b release/v1.2.0

# Only bug fixes allowed here — no new features
git commit -m "fix: last-minute release fix"
git commit -m "chore: bump version to 1.2.0"

# Merge into master and tag
git checkout master
git merge --no-ff release/v1.2.0
git tag -a v1.2.0 -m "v1.2.0: Release description"

# Merge back into develop to include release fixes
git checkout develop
git merge --no-ff release/v1.2.0

# Push everything
git push origin master develop --tags

# Delete release branch
git branch -d release/v1.2.0
git push origin --delete release/v1.2.0
```

---

### 3. Applying a Hotfix (Production Bug)

```bash
# Always branch from master (not develop!)
git checkout master
git pull origin master
git checkout -b hotfix/v1.0.2

# Fix the critical bug
git commit -m "fix(critical): patch description"

# Merge back into master and tag
git checkout master
git merge --no-ff hotfix/v1.0.2
git tag -a v1.0.2 -m "v1.0.2: Hotfix - critical bug fix"

# IMPORTANT: also merge into develop to keep it in sync
git checkout develop
git merge --no-ff hotfix/v1.0.2

# Push and clean up
git push origin master develop --tags
git branch -d hotfix/v1.0.2
git push origin --delete hotfix/v1.0.2
```

---

### 4. Finishing a Feature (Merge PR to develop)

```bash
git checkout develop
git merge --no-ff feature/your-feature-name
git push origin develop

# Delete feature branch after merge
git branch -d feature/your-feature-name
git push origin --delete feature/your-feature-name
```

---

## Commit Message Convention

Follow **Conventional Commits** format:

```
<type>(<scope>): <short description>

[optional body]
[optional footer]
```

### Types

| Type | When to use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code restructuring (no behavior change) |
| `docs` | Documentation only |
| `chore` | Build, CI, tooling |
| `test` | Adding or updating tests |
| `perf` | Performance improvement |

### Examples

```bash
git commit -m "feat(identity): add JWT refresh token endpoint"
git commit -m "fix(cart): fix race condition in CartRepository"
git commit -m "refactor(notification): extract IEmailService interface"
git commit -m "chore(docker): bump base image to .NET 10.1"
git commit -m "docs: update GITFLOW.md with new workflow examples"
```

---

## Current Active Branches

```bash
# View all branches
git branch -a

# View branch graph
git log --oneline --graph --all --decorate
```

---

## Rules

1. **Never commit directly to `master`** — only merge via release or hotfix branches
2. **Never commit directly to `develop`** — always use feature branches + PR
3. **`release/*` branches** — bug fixes only, no new features
4. **`hotfix/*` branches** — always merge into both `master` AND `develop`
5. **Tag every merge to `master`** with a semantic version tag
6. **Delete branches** after merging to keep the repo clean
