<script lang="ts">
    import {page} from "$app/stores";
    import {lastMonth as getLastMonth, monthFromNumber, nextMonth as getNextMonth} from "$lib/dateTools";
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

    const month = $derived(monthFromNumber($page.params.month, $page.params.year))
    const lastMonth = $derived(getLastMonth(month));
    const nextMonth = $derived(getNextMonth(month));

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
        categoryGroups: BudgetCategoryGroup[];
    };
    let loading = $state(load());
    let budget = $state<Budget>({
        id: "",
        month: 0,
        year: 0,
        expectedIncome: 0,
        actualIncome: 0,
        categoryGroups: []
    });
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
        const res = await apiClient.get<Budget>(`budgets/${$page.params.year}/${$page.params.month}`);
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
</script>

<div class="flex items-center mb-8">
    <a href="/budgets/{lastMonth.year}/{lastMonth.number}"
       class="btn btn-icon text-tertiary-500"
    >
        <Fa icon={faArrowLeft} />
    </a>
    <a href="/budgets/{nextMonth.year}/{nextMonth.number}"
       class="btn btn-icon text-tertiary-500"
    >
        <Fa icon={faArrowRight} />
    </a>
    <h1 class="h1">Budget for {month.name} {month.year}</h1>
</div>

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
        />
    </Card>
    <Card class="mb-8">
        <BudgetItem
            readonly
            name="Expenses"
            expected={totalExpected}
            actual={totalActual}
            barColor={categoryColor(totalExpected, totalActual)}
        >
            <div class="w-4"></div>
            <div>Left to allocate: {formatMoney(leftToAllocate)}</div>
            <div class="w-4"></div>
            <button onclick={addCategoryGroup} class="btn variant-outline-secondary">New Group</button>
        </BudgetItem>
    </Card>

    {#each budget.categoryGroups as group, groupIdx}
        <div class="card overflow-hidden mb-8">
            <div class="p-4 bg-surface-700">
                <BudgetItem
                    bind:name={group.name}
                    expected={group.limit}
                    expectedIsEditable={false}
                    actual={group.actual}
                    barColor={categoryColor(group.limit, group.actual)}
                    onSave={save}
                >
                    <button
                        class="btn btn-icon text-secondary-500"
                        onclick={() => addCategory(groupIdx)}
                    >
                        <Fa icon={faPlusCircle} />
                    </button>
                </BudgetItem>
            </div>
            <div class="p-4 pl-12">
                {#each group.categories as category}
                    <BudgetItem
                        bind:name={category.category}
                        bind:expected={category.limit}
                        actual={category.actual}
                        barColor={categoryColor(category.limit, category.actual)}
                        onSave={save}
                    >
                        <a href="/categories/{category.category}" class="btn btn-icon text-secondary-500">
                            <Fa icon={faArrowUpRightFromSquare} />
                        </a>
                    </BudgetItem>
                {/each}
            </div>
        </div>
    {/each}
{/await}
