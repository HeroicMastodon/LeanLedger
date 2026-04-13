<script lang="ts">
    import { Fa } from "svelte-fa";
    import { apiClient } from "$lib/apiClient";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import { formatMoney, sumUp } from "$lib";
    import { loadCategoryOptions } from "$lib/transactions";
    import BudgetItem from "$lib/budgets/BudgetItem.svelte";
    import Card from "$lib/components/Card.svelte";
    import { debounce } from "$lib/rules";
    import { faPlusCircle } from "@fortawesome/free-solid-svg-icons/faPlusCircle";
    import { faArrowUpRightFromSquare } from "@fortawesome/free-solid-svg-icons/faArrowUpRightFromSquare";
    import { faTrashCan } from "@fortawesome/free-solid-svg-icons/faTrashCan";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { faAngleUp } from "@fortawesome/free-solid-svg-icons/faAngleUp";
    import { faAngleDown } from "@fortawesome/free-solid-svg-icons/faAngleDown";
    import { faFolderPlus } from "@fortawesome/free-solid-svg-icons/faFolderPlus";
    import PiggyBankEntries from "$lib/piggybanks/PiggyBankEntries.svelte";
    import {
        defaultPiggyBankEntry,
        type PiggyBankEntry,
    } from "$lib/piggybanks";
    import PiggyBankDiffText from "$lib/piggybanks/PiggyBankDiffText.svelte";
    import Money from "$lib/components/Money.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import { faExternalLink, faPlus } from "@fortawesome/free-solid-svg-icons";
    import PiggyBankEntryForm from "$lib/piggybanks/PiggyBankEntryForm.svelte";
    import PiggyBanks from "$lib/metrics/PiggyBanks.svelte";

    type BudgetCategory = {
        category: string;
        limit: number;
        actual: number;
    };
    type BudgetCategoryGroup = {
        name: string;
        limit: number;
        actual: number;
        categories: BudgetCategory[];
    };
    type Budget = {
        id: string;
        month: number;
        year: number;
        expectedIncome: number;
        actualIncome: number;
        remainingExpenseTotal: number;
        categoryGroups: BudgetCategoryGroup[];
        unallocatedCategories: {
            name: string;
            amount: number;
        }[];
    };
    let loading = $state(Promise.resolve());
    let budget = $state<Budget>({
        id: "",
        month: 0,
        year: 0,
        expectedIncome: 0,
        actualIncome: 0,
        remainingExpenseTotal: 0,
        categoryGroups: [],
        unallocatedCategories: [],
    });
    let piggyBankEntries = $state<PiggyBankEntry[]>([]);

    $effect(() => {
        loading = load();
    });

    const piggyBankSum = $derived(sumUp(piggyBankEntries, (e) => e.amount));
    const piggyBankAllocations = $derived(
        sumUp(
            piggyBankEntries.filter((e) => e.amount > 0),
            (e) => e.amount,
        ),
    );
    const piggyBankDisbursements = $derived(
        sumUp(
            piggyBankEntries.filter((e) => e.amount < 0),
            (e) => e.amount,
        ),
    );
    const totalExpected = $derived(
        sumUp(budget.categoryGroups, (c) => c.limit),
    );
    const totalActual = $derived(
        sumUp(budget.categoryGroups, (c) => c.actual) +
            budget.remainingExpenseTotal,
    );
    const maxExpenses = $derived(
        totalActual > totalExpected ? totalActual : totalExpected,
    );
    const maxIncome = $derived(
        budget.expectedIncome > budget.actualIncome
            ? budget.expectedIncome
            : budget.actualIncome,
    );
    const leftToAllocate = $derived(maxIncome - totalExpected);
    const piggyBankDifference = $derived(
        maxIncome - maxExpenses - piggyBankSum,
    );

    const incomeColor = $derived.by(() => {
        if (budget.expectedIncome <= 0) {
            return "error";
        }
        const ratio = budget.actualIncome / budget.expectedIncome;

        if (ratio >= 0.9) {
            return "success";
        }

        if (ratio >= 0.4) {
            return "warning";
        }

        return "error";
    });

    let categoryOptions = $state<string[]>([]);

    async function load() {
        const res = await apiClient.get<Budget>(
            `budgets/${monthManager.selectedMonth.year}/${monthManager.selectedMonth.number}`,
        );
        budget = res.data;

        categoryOptions = await loadCategoryOptions();
        await loadPiggyBankEntries();
    }

    const save = debounce(async function () {
        const res = await apiClient.put<Budget>(`budgets/${budget.id}`, budget);
        budget = res.data;
    });

    function categoryColor(expected: number, actual: number) {
        if (expected < 0) return "error";
        if (!expected) return "success";
        const ratio = actual / expected;

        if (ratio > 1) {
            return "error";
        }

        if (ratio > 0.9) {
            return "warning";
        }

        return "success";
    }

    function addCategory(groupIndex: number) {
        budget.categoryGroups[groupIndex].categories.push({
            category: "New Category",
            limit: 0,
            actual: 0,
        });
        save();
    }

    async function removeCategory(groupIndex: number, categoryIndex: number) {
        budget.categoryGroups[groupIndex].categories.splice(categoryIndex, 1);
        save();
    }

    async function addCategoryGroup() {
        budget.categoryGroups.push({
            categories: [],
            name: "New Group",
            actual: 0,
            limit: 0,
        });
        save();
    }

    function removeCategoryGroup(index: number) {
        budget.categoryGroups.splice(index, 1);
        save();
    }

    function moveItemUp(arr: any[], idx: number) {
        if (idx == 0 || idx >= arr.length) {
            return;
        }
        const prev = arr[idx - 1];
        arr[idx - 1] = arr[idx];
        arr[idx] = prev;
        save();
    }

    function moveItemDown(arr: any[], idx: number) {
        if (idx < 0 || idx >= arr.length - 1) {
            return;
        }
        const next = arr[idx + 1];
        arr[idx + 1] = arr[idx];
        arr[idx] = next;
        save();
    }

    async function loadPiggyBankEntries() {
        try {
            const res = await apiClient.get<PiggyBankEntry[]>(
                `piggybanks/entries?${monthManager.params}`,
            );
            piggyBankEntries = res.data;
        } catch (e) {
            piggyBankEntries = [];
        }
    }

    let piggyEntry = $state<PiggyBankEntry>(defaultPiggyBankEntry());
    let entryError = $state<string>();

    async function savePiggyEntry() {
        entryError = undefined;

        if (!piggyEntry.piggyBank?.id) {
            entryError = "No piggy bank was specified."
            return false;
        }

        await apiClient.post(`piggybanks/${piggyEntry.piggyBank.id}/entries`, piggyEntry);
        piggyEntry = defaultPiggyBankEntry();
        await loadPiggyBankEntries();

        return true;
    }
</script>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <Card class="mb-4">
        <BudgetItem
            class="mb-4"
            name="Income"
            nameIsEditable={false}
            actual={budget.actualIncome}
            bind:expected={budget.expectedIncome}
            barColor={incomeColor}
            onSave={save}
            id="Income"
        />
        <BudgetItem
            readonly
            class="mb-4"
            name="Expenses"
            expected={totalExpected}
            actual={totalActual}
            barColor={categoryColor(totalExpected, totalActual)}
            id="Planned Expenses"
        >
            <button
                title="Add Expense Section"
                onclick={addCategoryGroup}
                class="btn text-secondary-500"
            >
                <Fa icon={faFolderPlus} />
            </button>
        </BudgetItem>
        <BudgetItem
            readonly
            name="Expense Vs Income"
            nameIsEditable={false}
            actual={totalActual}
            expected={maxIncome}
            barColor={categoryColor(maxIncome, totalActual)}
            onSave={save}
            id="Income"
        >
            <div class="w-4"></div>
            {#if maxIncome - totalActual < 0}
                <p class="text-error-500">
                    Overspent by {formatMoney(totalActual - maxIncome)}!
                </p>
            {:else if maxIncome - totalActual > 0}
                <p class="text-success-500">
                    Saving {formatMoney(maxIncome - totalActual)}!
                </p>
            {/if}
        </BudgetItem>
    </Card>
    <Card class="mb-4">
        <div class="flex gap-4 items-center justify-start mb-4 flex-wrap">
            <h4 class="h4">Piggy Bank Entries</h4>
            <div>
                Allocated: <Money amount={piggyBankAllocations} />
            </div>
            <div>
                Disbursed: <Money amount={piggyBankDisbursements} />
            </div>
            <div>
                Change: <Money amount={piggyBankSum} />
            </div>
        </div>
        <div class="mb-2 flex gap-4 items-center">
            <PiggyBankDiffText difference={piggyBankDifference} />
            <FormButton
                class="btn-icon-sm p-2 variant-filled-secondary size-5"
                icon={faPlus}
                text="New Entry"
                onConfirm={savePiggyEntry}
                error={entryError}
            >
                <PiggyBankEntryForm bind:entry={piggyEntry} />
            </FormButton>
            <a
                href="/piggybanks"
                title="Piggy Banks"
                class="btn btn-icon-sm p-2 text-secondary-500"
            >
                <Fa icon={faExternalLink} />
            </a>
        </div>
        <PiggyBankEntries entries={piggyBankEntries} showPiggyBank entrySaved={loadPiggyBankEntries} />
    </Card>
    <Card class="mb-4">
        <BudgetItem
            class="mb-4"
            readonly
            name="Unplanned Expenses"
            expected={leftToAllocate}
            actual={budget.remainingExpenseTotal}
            barColor={categoryColor(
                leftToAllocate,
                budget.remainingExpenseTotal,
            )}
            id="Unallocated Expenses"
        ></BudgetItem>
        {#each budget.unallocatedCategories as category}
            <BudgetItem
                class="pl-12 mb-4"
                readonly
                name={category.name}
                actual={category.amount}
                expected={0}
                barColor="warning"
                id={category.name}
            >
                <a
                    href="/categories/{encodeURIComponent(category.name)}"
                    class="btn btn-icon text-secondary-500"
                >
                    <Fa icon={faArrowUpRightFromSquare} />
                </a>
            </BudgetItem>
        {/each}
    </Card>

    {#each budget.categoryGroups as group, groupIdx}
        <div class="card overflow-hidden mb-4">
            <div class="p-4 bg-surface-700 flex gap-4 items-center">
                <BudgetItem
                    class="grow"
                    bind:name={group.name}
                    expected={group.limit}
                    expectedIsEditable={false}
                    actual={group.actual}
                    barColor={categoryColor(group.limit, group.actual)}
                    onSave={save}
                    id={groupIdx.toString()}
                >
                    <button
                        class="btn btn-icon text-secondary-500"
                        onclick={() => addCategory(groupIdx)}
                    >
                        <Fa icon={faPlusCircle} />
                    </button>
                    <button
                        onclick={() => removeCategoryGroup(groupIdx)}
                        class="btn btn-icon text-error-500"
                    >
                        <Fa icon={faTrashCan} />
                    </button>
                </BudgetItem>
                <div class="hidden md:flex flex-col">
                    {#if groupIdx > 0}
                        <button
                            onclick={() =>
                                moveItemUp(budget.categoryGroups, groupIdx)}
                            class="btn"
                        >
                            <Fa icon={faAngleUp} />
                        </button>
                    {/if}
                    {#if groupIdx < budget.categoryGroups.length - 1}
                        <button
                            onclick={() =>
                                moveItemDown(budget.categoryGroups, groupIdx)}
                            class="btn"
                        >
                            <Fa icon={faAngleDown} />
                        </button>
                    {/if}
                </div>
            </div>
            <div class="p-4 pl-12">
                {#each group.categories as category, categoryIdx}
                    <div class="flex gap-8 items-center">
                        <BudgetItem
                            class="mb-4 grow"
                            bind:name={category.category}
                            bind:expected={category.limit}
                            actual={category.actual}
                            barColor={categoryColor(
                                category.limit,
                                category.actual,
                            )}
                            onSave={save}
                            id="{groupIdx}-{categoryIdx}"
                            options={categoryOptions}
                        >
                            <a
                                href="/categories/{encodeURIComponent(
                                    category.category,
                                )}"
                                class="btn btn-icon text-secondary-500"
                            >
                                <Fa icon={faArrowUpRightFromSquare} />
                            </a>
                            <button
                                onclick={() =>
                                    removeCategory(groupIdx, categoryIdx)}
                                class="btn btn-icon text-error-500"
                            >
                                <Fa icon={faTrashCan} />
                            </button>
                        </BudgetItem>
                        <div class="hidden md:flex flex-col">
                            {#if categoryIdx > 0}
                                <button
                                    onclick={() =>
                                        moveItemUp(
                                            group.categories,
                                            categoryIdx,
                                        )}
                                    class="btn"
                                >
                                    <Fa icon={faAngleUp} />
                                </button>
                            {/if}
                            {#if categoryIdx < group.categories.length - 1}
                                <button
                                    onclick={() =>
                                        moveItemDown(
                                            group.categories,
                                            categoryIdx,
                                        )}
                                    class="btn"
                                >
                                    <Fa icon={faAngleDown} />
                                </button>
                            {/if}
                        </div>
                    </div>
                {/each}
            </div>
        </div>
    {/each}
{/await}
