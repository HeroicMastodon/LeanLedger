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
    import {formatMoney} from "$lib";
    import PredictiveText from "$lib/components/forms/PredictiveText.svelte";
    import {loadCategoryOptions} from "$lib/transactions";

    const month = $derived(monthFromNumber($page.params.month, $page.params.year))
    const lastMonth = $derived(getLastMonth(month));
    const nextMonth = $derived(getNextMonth(month));

    type BudgetCategory = {
        category: string;
        limit: number;
        actual: number;
    }
    type Budget = {
        id: string;
        month: number;
        year: number;
        expectedIncome: number;
        actualIncome: number;
        categories: BudgetCategory[];
    };
    let loading = $state(load());
    let budget = $state<Budget>({
        id: "",
        month: 0,
        year: 0,
        expectedIncome: 0,
        actualIncome: 0,
        categories: []
    });
    const leftToAllocate = $derived(
        budget.expectedIncome
        - budget.categories
            .reduce((prev, curr) => prev + curr.limit, 0)
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
        if (category.limit) return "success";
        const ratio = category.actual / category.limit;

        if (ratio > 1) {
            return "error";
        }

        if (ratio > .9) {
            return "warning";
        }

        return "success";
    }

    function addCategory() {
        budget.categories.push({category: "New Category", limit: 0, actual: 0})
    }
</script>

<div class="flex items-center gap-4 mb-8 justify-between">
    <div class="flex gap-4 items-center">
        <h1 class="h1">Budget for {month.name} {month.year}</h1>
        <button
            onclick={save}
            class="btn variant-filled-primary"
        >Save
        </button>
    </div>

    <div class="flex gap-4 items-center">
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
    </div>
</div>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <h2 class="h2 mb-4">Income</h2>
    <div class="flex gap-4 items-center mb-4">
        <MoneyInput
            label="Expected"
            bind:value={budget.expectedIncome}
        />
        <MoneyInput
            readonly
            label="Actual"
            value={budget.actualIncome}
        />
    </div>
    <div class="flex flex-col gap-4 mb-8">
        <ProgressBar
            meter="bg-{incomeColor}-500"
            track="bg-{incomeColor}-500/30"
            min={0}
            max={budget.expectedIncome}
            value={budget.actualIncome}
            height="h-4"
        />
    </div>
    <h2 class="h2 mb-8">Total Expenditures</h2>
    <div class="flex flex-row gap-4 items-center mb-8">
        <div>
            Total Expected: {formatMoney(0)}
        </div>
        <div>
            Left To Allocate:
            <Money amount={0} />
        </div>
        <div>
            <!--            TODO: figure out how to color this properly-->
            Actual Spent:
            <Money amount={0} />
        </div>
        <div>
            Remaining to Spend:
            <Money amount={0} />
        </div>
    </div>
    <div class="flex flex-row gap-4 items-center mb-8">
        <h3 class="h3">Categories</h3>
        <button onclick={addCategory} class="btn variant-outline-primary">Add</button>
    </div>
    {#each budget.categories as category, idx}
        <div class="flex gap-4 items-end ml-8 mb-4">
            <PredictiveText
                label="Name"
                inputId="category-{idx}"
                datalistId="category-list-{idx}"
                bind:value={category.category}
                options={categoryOptions}
            />
            <MoneyInput
                label="Limit"
                bind:value={category.limit}
            />
            <MoneyInput
                readonly
                label="Actual"
                value={category.actual}
            />
            <MoneyInput
                readonly
                label="Remaining"
                value={category.limit - category.actual}
            />
        </div>
        <div class="flex flex-col gap-4 ml-8 mb-4">
            <ProgressBar
                meter="bg-{categoryColor(category)}-500"
                track="bg-{categoryColor(category)}-500/30"
                min={0}
                max={category.limit}
                value={category.actual}
                height="h-4"
            />
        </div>
    {/each}
{/await}
