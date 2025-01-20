<script lang="ts">
    import {page} from "$app/stores";
    import {monthFromNumber, lastMonth as getLastMonth, nextMonth as getNextMonth} from "$lib/dateTools";
    import {Fa} from "svelte-fa";
    import {faArrowLeft} from "@fortawesome/free-solid-svg-icons/faArrowLeft";
    import {faArrowRight} from "@fortawesome/free-solid-svg-icons/faArrowRight";
    import {apiClient} from "$lib/apiClient";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import Money from "$lib/components/Money.svelte";
    import {formatMoney, sumUp} from "$lib";
    import PredictiveText from "$lib/components/forms/PredictiveText.svelte";
    import {loadCategoryOptions} from "$lib/transactions";
    import BudgetItem from "$lib/budgets/BudgetItem.svelte";
    import Card from "$lib/components/Card.svelte";

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
    const leftToAllocate = $derived(
        budget.expectedIncome - sumUp(budget.categoryGroups, c => c.limit)
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

    async function save() {
        const res = await apiClient.put<Budget>(`budgets/${budget.id}`, budget);
        budget = res.data;
    }

    function categoryColor(category: BudgetCategory) {
        if (!category.limit) return "success";
        const ratio = category.actual / category.limit;

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
    }

    function removeCategory(groupIndex: number, categoryIndex: number) {
        budget.categoryGroups[groupIndex].categories.splice(categoryIndex, 1)
    }

    function addCategoryGroup() {
        budget.categoryGroups.push({
            categories: [],
            name: "New Group",
            actual: 0,
            limit: 0
        })
    }

    function removeCategoryGroup(index: number) {
        budget.categoryGroups.splice(index, 1);
    }
</script>

<div class="flex items-center gap-4 mb-8">
    <a href="/budgets/{lastMonth.year}/{lastMonth.number}"
       class="btn btn-icon variant-outline-tertiary text-tertiary-500"
    >
        <Fa icon={faArrowLeft} />
    </a>
    <a href="/budgets/{nextMonth.year}/{nextMonth.number}"
       class="btn btn-icon variant-outline-tertiary text-tertiary-500"
    >
        <Fa icon={faArrowRight} />
    </a>
    <h1 class="h1">Budget for {month.name} {month.year}</h1>
    <button
        onclick={save}
        class="btn variant-filled-primary"
    >Save
    </button>
</div>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <Card>
        <BudgetItem
            name="Income"
            nameIsEditable={false}
            actual={budget.actualIncome}
            bind:expected={budget.expectedIncome}
            barColor={incomeColor}
            onSave={save}
        />
    </Card>

<!--    <h2 class="h2 mb-8">Total Expenditures</h2>-->
<!--    <div class="flex flex-row gap-4 items-center mb-8">-->
<!--        <div>-->
<!--            Total Expected: {formatMoney(sumUp(budget.categories, c => c.limit))}-->
<!--        </div>-->
<!--        <div>-->
<!--            Left To Allocate:-->
<!--            <Money amount={budget.expectedIncome - sumUp(budget.categories, c => c.limit)} />-->
<!--        </div>-->
<!--        <div>-->
<!--            &lt;!&ndash;            TODO: figure out how to color this properly-->
<!--            TODO: make some useful derived properties here-->
<!--            &ndash;&gt;-->
<!--            Total Spent:-->
<!--            <Money amount={sumUp(budget.categories, c => c.actual)} />-->
<!--        </div>-->
<!--        <div>-->
<!--            Left Over:-->
<!--            <Money amount={budget.expectedIncome - sumUp(budget.categories, c => c.actual)} />-->
<!--        </div>-->
<!--    </div>-->


<!--    <div class="flex flex-row gap-4 items-center mb-8">-->
<!--        <h3 class="h3">Categories</h3>-->
<!--        <button onclick={addCategory} class="btn variant-outline-primary">Add</button>-->
<!--    </div>-->
<!--    {#each budget.categories as category, idx}-->
<!--        <div class="flex gap-4 items-end ml-8 mb-4">-->
<!--            <PredictiveText-->
<!--                label="Name"-->
<!--                inputId="category-{idx}"-->
<!--                datalistId="category-list-{idx}"-->
<!--                bind:value={category.category}-->
<!--                options={categoryOptions}-->
<!--            />-->
<!--            <MoneyInput-->
<!--                label="Limit"-->
<!--                bind:value={category.limit}-->
<!--            />-->
<!--            <MoneyInput-->
<!--                readonly-->
<!--                label="Actual"-->
<!--                value={category.actual}-->
<!--            />-->
<!--            <MoneyInput-->
<!--                readonly-->
<!--                label="Remaining"-->
<!--                value={category.limit - category.actual}-->
<!--            />-->
<!--        </div>-->
<!--        <div class="flex flex-col gap-4 ml-8 mb-4">-->
<!--            <ProgressBar-->
<!--                meter="bg-{categoryColor(category)}-500"-->
<!--                track="bg-{categoryColor(category)}-500/30"-->
<!--                min={0}-->
<!--                max={category.limit}-->
<!--                value={category.actual}-->
<!--                height="h-4"-->
<!--            />-->
<!--        </div>-->
<!--    {/each}-->
{/await}
