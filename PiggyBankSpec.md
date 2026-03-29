Piggy Banks Spec
- Purpose — Add standalone piggy banks that aggregate allocations from transactions; support CRUD, transaction allocations (positive only), dashboard metrics by month view, and a rule action to create allocations (rule UI uses two inputs: piggy id + amount). No multi-user, no currencies.
Model & DB
- Files to add:
  - LeanLedgerServer/PiggyBanks/PiggyBank.cs
  - LeanLedgerServer/PiggyBanks/PiggyAllocation.cs
  - Add EF migration under LeanLedgerServer/Migrations/{timestamp}_AddPiggyBanks.cs
- Entities
  - PiggyBank
    - Id: Guid (PK)
    - Name: string (required)
    - InitialBalance: decimal (not nullable, default 0)
    - BalanceTarget: decimal? (nullable)
    - Closed: bool (default false)
    - Note: do NOT add timestamps unless requested
  - PiggyAllocation
    - Id: Guid (PK)
    - TransactionId: Guid (FK → Transactions.Transaction.Id) — allocations always tied to a transaction
    - PiggyBankId: Guid (FK → PiggyBanks.PiggyBank.Id)
    - Amount: decimal (must be > 0)
- DB constraints & indexes
  - FK PiggyAllocation.TransactionId → Transactions(Id) ON DELETE NO ACTION (allocations remain but metrics ignore deleted transactions)
  - FK PiggyAllocation.PiggyBankId → PiggyBanks(Id) ON DELETE NO ACTION
  - Index on PiggyAllocation.TransactionId and PiggyAllocation.PiggyBankId
- Balance computation rule (DB-level intent)
  - Piggy balance = InitialBalance + SUM(PiggyAllocation.Amount) across allocations whose transaction is counted for the requested month (see QueryByMonth logic).
  - Allocations stored positive only.
API endpoints (server side)
- Routing convention: add baseRoute.MapPiggyBanks() in Program.cs next to other Map* calls.
- Piggy banks CRUD (controller LeanLedgerServer/PiggyBanks/Endpoints.cs)
  - GET /api/piggy-banks?Month=&Year= (accept QueryByMonth byMonth from query): list all piggy banks with balances computed as of that byMonth (cumulative up to and including the month when byMonth provided; use QueryTransactionsByMonth(query, byMonth) as other controllers do)
    - Response item: { id, name, initialBalance, balanceTarget, balance, progressPercent, closed }
  - GET /api/piggy-banks/{id}?Month=&Year= : single piggy bank detail (include list of allocations optionally paged; see below)
  - POST /api/piggy-banks
    - Body: { name: string, initialBalance?: decimal (default 0), balanceTarget?: decimal | null }
    - 201 Created with created entity
  - PUT /api/piggy-banks/{id}
    - Body: same as POST; updates fields (except Id)
    - 200 Ok with updated entity
  - DELETE /api/piggy-banks/{id}
    - Semantic delete: set Closed = true (preserve historic allocations). Return 204 NoContent.
- Allocations endpoints (under transactions)
  - GET /api/transactions/{transactionId}/allocations
    - Returns list of allocations for that transaction (include piggy bank id/name)
  - POST /api/transactions/{transactionId}/allocations
    - Body: { piggyBankId: Guid, amount: decimal } (amount > 0)
    - Validations (see Validation section)
    - Responses: 201 Created (allocation), 400 Bad Request with validation details
  - PUT /api/transactions/{transactionId}/allocations/{allocationId}
    - Body: { piggyBankId: Guid, amount: decimal }
    - Allows reassigning to a different piggy and/or changing amount. Validations apply.
  - DELETE /api/transactions/{transactionId}/allocations/{allocationId}
    - Removes the allocation (204 NoContent)
- Metrics endpoint (controller change: LeanLedgerServer/Metrics/MetricsController.cs)
  - Add GET /api/metrics/piggy-banks?Month=&Year= (accepts QueryByMonth byMonth)
    - Response: { piggyBanks: [{ id, name, balance, target, progressPercent }], totals: { piggyTotal, accountTotal, piggyTotalExceedsAccounts: bool } }
    - balance computed as InitialBalance + SUM(alloc.Amount) for allocations whose Transaction is included by QueryTransactionsByMonth and where Transaction.IsDeleted == false.
    - progressPercent = if target null then null else (balance / target) * 100 (allow >100).
    - piggyTotalExceedsAccounts is true when SUM(balance of all piggy banks) > SUM(balance of all accounts included by same byMonth) (use existing account balance logic).
Validation & Business rules (server enforced)
- Allocation amount must be > 0 (positive-only).
- Allocations cannot be attached to Transfer transactions (Transaction.Type == Transfer) — return 400 Bad Request.
- Sum of allocations attached to a transaction (including existing allocations plus the new/updated allocation) MUST be ≤ transaction.Amount. Use the transaction.Amount value (transactions are stored as positive numbers).
  - In update, compute current sum excluding the allocation being updated, add new amount, compare ≤ transaction.Amount.
- PiggyBank referenced by allocation must exist and not be logically closed? — allow allocations to closed piggy banks? Default: disallow creating new allocations on a piggy with Closed = true (return 400); existing allocations remain.
- Transactions with IsDeleted == true should not be returned in metrics/balance calculations; creating allocations on deleted transactions should be disallowed (400).
- No transfers entity; allocations are standalone records pointing to transactions.
- Server should return structured validation errors consistent with existing InvalidRequest usage in TransactionFunctions.ValidateTransaction.
QueryByMonth & balance queries
- Use LeanLedgerServer/Common/QueryByMonth.cs functions:
  - To include a transaction for piggy balance as of a month, call QueryTransactionsByMonth(queryOfTransactions, byMonth) with givenMonthOnly = false (default) — this mirrors account balance behavior where balances are cumulative up to the month viewed.
- Example LINQ (for piggy single balance):
  - Start from piggy banks:
    - db.PiggyBanks.Where(pb => !pb.Closed)
      .Select(pb => new {
        pb.Id,
        pb.Name,
        InitialBalance = pb.InitialBalance,
        AllocSum = db.PiggyAllocations
          .Where(a => a.PiggyBankId == pb.Id)
          .Join(db.Transactions, a => a.TransactionId, t => t.Id, (a,t) => new { a.Amount, t })
          .Where(x => !x.t.IsDeleted)
          .Let(q => QueryTransactionsByMonth(q.Select(x => x.t), byMonth)) // conceptual; implement as join+filter on t.Date
          .Sum(x => x.Amount)
      })
    - Final balance = InitialBalance + AllocSum
- For aggregated piggyTotal and accountTotal compute sums via grouping/joins and reuse existing Accounts balance logic from LeanLedgerServer/Accounts/AccountFunctions.cs.
Rule engine integration
- Goal: Add a rule action that creates an allocation attached to the transaction being processed. UI must present two separate fields for this action: piggyBankId (Guid selector) and amount (decimal).
- Code-level plan:
  - Add a new enum value: RuleActionType.AddPiggyAllocation.
  - Extend RuleAction model to hold two extra optional properties when ActionType == AddPiggyAllocation:
    - Guid? PiggyBankId
    - decimal? Amount
  - Serialization: maintain existing JSON shape used by rules storage; update RulesDbConfiguration to deserialize/serialize new fields.
  - When rule is applied (RuleAction.ApplyTo(Transaction)), for AddPiggyAllocation:
    - Validate that Transaction is not a transfer (Transactions.Type != Transfer).
    - Validate transaction exists (it is the transaction object in memory) and not deleted.
    - Validate PiggyBankId corresponds to an existing piggy bank and it is not closed.
    - Validate current allocations sum + Amount ≤ transaction.Amount (note: rule application happens during creation flow in TransactionFunctions.CreateNewTransaction before DB insert; the code there has access to transactions IQueryable and db context in endpoints; design choice:)
      - Preferred approach: In TransactionFunctions.CreateNewTransaction we can allow rules to mutate the in-memory Transaction and also produce a list of pending allocations to persist after transaction Save (so we can run validations and persist in one DB transaction). The spec will require that rule application provide a set of intended allocations back to the caller so the endpoint can validate/persist them.
  - Error handling: If a rule action would violate invariants, rules application must cause the transaction creation to fail with a BadRule / InvalidRequest style error (consistent with existing BadRule behavior).
- Front-end changes:
  - LeanLedgerApp/src/lib/rules/index.ts add AddPiggyAllocation action type and default form fields.
  - Rules UI page routes/rules/[id]/+page.svelte add condition to render PiggyBank selector and Amount input when action type selected.
API DTOs & sample payloads
- Create piggy bank (POST /api/piggy-banks)
  - Request:
        { name: Vacation, initialBalance: 100.00, balanceTarget: 1000.00 }
      - Response 201:
        { id: GUID, name: Vacation, initialBalance: 100.00, balanceTarget: 1000.00, closed: false }
    - Create allocation (POST /api/transactions/{txId}/allocations)
  - Request:
        { piggyBankId: GUID, amount: 25.00 }
      - Errors:
    - 400 if transaction.Type == Transfer
    - 400 if transaction.IsDeleted == true
    - 400 if piggy is Closed
    - 400 if amount <= 0
    - 400 if (existingAllocSum + amount) > transaction.Amount
- Metrics response example (GET /api/metrics/piggy-banks?Month=3&Year=2026):
    {
    piggyBanks: [
      { id:..,name:Vacation,balance:125.00,target:1000.00,progressPercent:12.5 }
    ],
    totals: { piggyTotal: 125.00, accountTotal: 800.00, piggyTotalExceedsAccounts: false }
  }
  
Validation error shape
- Use existing InvalidRequest pattern (see TransactionFunctions.ValidateTransaction) to return field-specific messages. For example:
  - 400 Bad Request with { title: "Invalid Request", detail: "Allocations exceed transaction amount", extensions: { field: "amount" } } (consistent with repository patterns).
UI/UX (front-end changes - API only unless requested)
- New UI tab: Piggy Banks (route e.g., /piggy-banks) showing table with columns: Name, Balance, Target, Progress%, Actions (view/edit/close).
- Piggy detail page: shows InitialBalance, Target, current Balance, progress bar, and list of allocations (with linked transaction entries).
- Transaction screen: show list of attached allocations and allow add/edit/delete (backing API endpoints described above).
- Dashboard metrics panel: call GET /api/metrics/piggy-banks?Month=&Year= and show small table (top N piggy banks) and indicator if piggyTotalExceedsAccounts true (highlight).
- Rule UI: new action type with two separate inputs: Piggy selection (dropdown of id/name) and amount (decimal input).
Tests (add under Tests/)
- Unit / integration tests to add:
  1. Tests/PiggyBanks/PiggyBalanceTests.cs
     - TestBalance_InitialPlusAllocations_ByMonth: create piggy with initialBalance, create transactions at various dates with allocations, assert computed balance for specific month matches expected cumulative value.
  2. Tests/PiggyBanks/AllocationValidationTests.cs
     - CannotAddAllocation_ToTransferTransaction: ensure 400 when trying to add allocation to a transfer.
     - CannotAddAllocation_ExceedTransactionAmount: pre-add allocations that sum close to transaction.Amount and assert adding new allocation that would exceed fails.
     - UpdateAllocation_AdjustsSumsCorrectly: update allocation amounts and ensure validation uses sum excluding the updated alloc.
     - CannotAddAllocation_ToClosedPiggy: ensure 400 when piggy.Closed == true.
  3. Tests/Metrics/PiggyMetricsTests.cs
     - Metrics_ReturnsProgressAndTotals: verify progressPercent and piggyTotalExceedsAccounts computed correctly.
  4. Tests/Automation/RuleAction/AddPiggyAllocationTests.cs
     - RuleAction_AddPiggyAllocation_AppliesAndPersists: define rule action with PiggyBankId + Amount, run it against transaction creation flow and assert allocation persisted when valid.
     - RuleAction_AddPiggyAllocation_ViolationFailsTransaction: rule action that would cause allocations to exceed transaction.Amount causes transaction creation to fail with BadRule (or InvalidRequest) consistent with rules error patterns.
- Test runner: use existing dotnet test Tests/Tests.csproj with filtered tests; mock DB via SQLite in-memory or reuse patterns in repository tests.
Server flow changes (high-level)
- Transaction creation endpoint (Transactions/Endpoints.CreateTransaction) currently:
  - Calls TransactionFunctions.CreateNewTransaction which applies rules to the in-memory Transaction, then persists Transaction.
- New flow for allocations from rules:
  - Option A (preferred): extend CreateNewTransaction to return both the validated Transaction and a list of pending PiggyAllocation DTOs (created from rule actions and/or user-provided allocations). Endpoint persists the Transaction, then validates & persists allocations in same DB context and SaveChangesAsync() before returning created response. If allocation validation fails, rollback and return an error (throw or return Err). This ensures atomicity.
  - Option B: have rule actions mutate Transaction and also append to an in-memory List<PiggyAllocation> on the Transaction object (a temporary property used only during create flow), then endpoint persists both.
- The spec chooses Option A: CreateNewTransaction returns Result<(Transaction transaction, List<PendingAllocation> allocations)> where PendingAllocation = { PiggyBankId, Amount }. Endpoint will then validate allocations vs transaction.Amount and persist them.
Open/clarifying decision (defaults used)
- Allocations are positive-only and validation uses simple comparison: existingAllocSum + newAllocAmount ≤ transaction.Amount (transaction.Amount stored positive).
- Delete piggy sets Closed = true (we will not physically delete rows). Creating allocations on closed piggies disallowed.
- Allocations persist even if their transaction is later soft-deleted; metrics and balance calculations ignore deleted transactions.
Files to change / add (summary)
- Add: LeanLedgerServer/PiggyBanks/PiggyBank.cs
- Add: LeanLedgerServer/PiggyBanks/PiggyAllocation.cs
- Add: LeanLedgerServer/PiggyBanks/Endpoints.cs
- Add: migration: LeanLedgerServer/Migrations/{timestamp}_AddPiggyBanks.cs
- Update: Program.cs add baseRoute.MapPiggyBanks();
- Update: LeanLedgerServer/Metrics/MetricsController.cs add GET /api/metrics/piggy-banks
- Update: LeanLedgerServer/Transactions/Endpoints.cs add allocation endpoints for transactions
- Update: LeanLedgerServer/Automation/RuleAction.cs (or add new action type) to support AddPiggyAllocation with PiggyBankId + Amount fields
- Update: front-end rule helpers LeanLedgerApp/src/lib/rules/index.ts and rule editor routes/rules/[id]/+page.svelte to support new action inputs
- Tests: add files described above under Tests/
Acceptance criteria (what to verify in tests/manual QA)
- Creating a piggy bank persists entity and shows correct initial values.
- Adding allocation to expense/income transactions obeys invariants (not on transfers; sum allocations ≤ transaction.Amount; piggy not closed).
- Balances computed for a byMonth are cumulative up to that month (use QueryByMonth behavior).
- Metrics endpoint returns correct per-piggy balances, progress percent, and global piggyTotalExceedsAccounts boolean.
- Rule action allows creating allocations during transaction creation and fails transaction creation when allocation would violate invariants.
