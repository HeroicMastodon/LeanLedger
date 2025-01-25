<script lang="ts">
    import {page} from "$app/stores";
    import {Fa} from "svelte-fa";
    import {faArrowLeft} from "@fortawesome/free-solid-svg-icons/faArrowLeft";
    import {faArrowRight} from "@fortawesome/free-solid-svg-icons/faArrowRight";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {formatMoney, sumUp} from "$lib";
    import {loadCategoryOptions} from "$lib/transactions";
    import BudgetItem from "$lib/budgets/BudgetItem.svelte";
    import Card from "$lib/components/Card.svelte";
    import {debounce} from "$lib/rules";
    import {faPlusCircle} from "@fortawesome/free-solid-svg-icons/faPlusCircle";
    import {faArrowUpRightFromSquare} from "@fortawesome/free-solid-svg-icons/faArrowUpRightFromSquare";
    import {afterNavigate} from "$app/navigation";
    import {faTrashCan} from "@fortawesome/free-solid-svg-icons/faTrashCan";
    import {monthManager} from "$lib/selectedMonth.svelte";
    import {faAngleUp} from "@fortawesome/free-solid-svg-icons/faAngleUp";
    import {faAngleDown} from "@fortawesome/free-solid-svg-icons/faAngleDown";

    type BudgetCategory = {
        category: string;
        limit: number;
        actual: number;
    }
    type BudgetCategoryGroup = {
        name: string;
        limit: number;
        actual: number;
        categories: BudgetCategory[],
    }
    type Budget = {
        id: string;
        month: number;
        year: number;
        expectedIncome: number;
        actualIncome: number;
        remainingExpenseTotal: number;
        categoryGroups: BudgetCategoryGroup[];
    };
    let loading = $state(load());
    let budget = $state<Budget>({
        id: "",
        month: 0,
        year: 0,
        expectedIncome: 0,
        actualIncome: 0,
        remainingExpenseTotal: 0,
        categoryGroups: []
    });

    $effect(() => {
        loading = load();
    })


    const totalExpected = $derived(sumUp(budget.categoryGroups, c => c.limit));
    const totalActual = $derived(sumUp(budget.categoryGroups, c => c.actual));
    const leftToAllocate = $derived(
        budget.expectedIncome - totalExpected
    );

    const incomeColor = $derived.by(() => {
        if (budget.expectedIncome <= 0) {
            return 'error';
        }
        const ratio = budget.actualIncome / budget.expectedIncome;

        if (ratio >= .9) {
            return 'success';
        }

        if (ratio >= .4) {
            return 'warning';
        }

        return 'error';
    });

    let categoryOptions = $state<string[]>([]);

    async function load() {
        const res = await apiClient.get<Budget>(`budgets/${monthManager.selectedMonth.year}/${monthManager.selectedMonth.number}`);
        budget = res.data;

        categoryOptions = await loadCategoryOptions();
    }

    const save = debounce(async function () {
        const res = await apiClient.put<Budget>(`budgets/${budget.id}`, budget);
        budget = res.data;
    });

    function categoryColor(expected: number, actual: number) {
        if (!expected) return "success";
        const ratio = actual / expected;

        if (ratio > 1) {
            return "error";
        }

        if (ratio > .9) {
            return "warning";
        }

        return "success";
    }

    function addCategory(groupIndex: number) {
        budget.categoryGroups[groupIndex].categories.push({
            category: "New Category", limit: 0, actual: 0
        })
        save();
    }

    async function removeCategory(groupIndex: number, categoryIndex: number) {
        budget.categoryGroups[groupIndex].categories.splice(categoryIndex, 1)
        save();
    }

    async function addCategoryGroup() {
        budget.categoryGroups.push({
            categories: [],
            name: "New Group",
            actual: 0,
            limit: 0
        })
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
</script>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <Card class="mb-8">
        <BudgetItem
            name="Income"
            nameIsEditable={false}
            actual={budget.actualIncome}
            bind:expected={budget.expectedIncome}
            barColor={incomeColor}
            onSave={save}
            id="Income"
        />
    </Card>
    <Card class="mb-8">
        <BudgetItem
            class="mb-4"
            readonly
            name="Planned Expenses"
            expected={totalExpected}
            actual={totalActual}
            barColor={categoryColor(totalExpected, totalActual)}
            id="Planned Expenses"
        >
            <div class="w-4"></div>
            <div>Left to allocate: {formatMoney(leftToAllocate)}</div>
            <div class="w-4"></div>
            <button onclick={addCategoryGroup} class="btn variant-outline-secondary">New Group</button>
        </BudgetItem>
        <BudgetItem
            readonly
            name="Unallocated Expenses"
            expected={leftToAllocate}
            actual={budget.remainingExpenseTotal}
            barColor={categoryColor(leftToAllocate, budget.remainingExpenseTotal)}
            id="Unallocated Expenses"
        ></BudgetItem>
    </Card>

    {#each budget.categoryGroups as group, groupIdx}
        <div class="card overflow-hidden mb-8">
            <div class="p-4 bg-surface-700 flex gap-8 items-center">
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
                    <button onclick={() => removeCategoryGroup(groupIdx)} class="btn btn-icon text-error-500">
                        <Fa icon={faTrashCan} />
                    </button>
                </BudgetItem>
                <div class="flex flex-col">
                    {#if groupIdx > 0}
                        <button
                            onclick={() => moveItemUp(budget.categoryGroups, groupIdx)}
                            class="btn"
                        >
                            <Fa icon={faAngleUp} />
                        </button>
                    {/if}
                    {#if groupIdx < budget.categoryGroups.length - 1}
                        <button
                            onclick={() => moveItemDown(budget.categoryGroups, groupIdx)}
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
                            barColor={categoryColor(category.limit, category.actual)}
                            onSave={save}
                            id="{groupIdx}-{categoryIdx}"
                            options={categoryOptions}
                        >
                            <a href="/categories/{category.category}" class="btn btn-icon text-secondary-500">
                                <Fa icon={faArrowUpRightFromSquare} />
                            </a>
                            <button onclick={() => removeCategory(groupIdx, categoryIdx)}
                                    class="btn btn-icon text-error-500"
                            >
                                <Fa icon={faTrashCan} />
                            </button>
                        </BudgetItem>
                        <div class="flex flex-col">
                            {#if categoryIdx > 0}
                                <button
                                    onclick={() => moveItemUp(group.categories, categoryIdx)}
                                    class="btn"
                                >
                                    <Fa icon={faAngleUp} />
                                </button>
                            {/if}
                            {#if categoryIdx < group.categories.length - 1}
                                <button
                                    onclick={() => moveItemDown(group.categories, categoryIdx)}
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
