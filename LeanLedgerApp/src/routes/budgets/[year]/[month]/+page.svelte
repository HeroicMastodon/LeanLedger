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

    const month = $derived(monthFromNumber($page.params.month, $page.params.year))
    const lastMonth = $derived(getLastMonth(month));
    const nextMonth = $derived(getNextMonth(month));

    type Budget = {
        id: string;
        month: number;
        year: number;
        expectedIncome: number;
        actualIncome:  number;
    };
    let loading = $state(load());
    let budget = $state<Budget>({
        id: "",
        month: 0,
        year: 0,
        expectedIncome: 0,
        actualIncome: 0
    });

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

    async function load() {
        const res = await apiClient.get<Budget>(`budgets/${$page.params.year}/${$page.params.month}`);
        budget = res.data;
    }
    async function save() {
        const res = await apiClient.put<Budget>(`budgets/${budget.id}`, budget);
        budget = res.data;
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
            bind:value={budget.actualIncome}
        />
    </div>
    <div class="flex flex-col gap-4">
        <ProgressBar
            meter="bg-{incomeColor}-500"
            track="bg-{incomeColor}-500/30"
            min={0}
            max={budget.expectedIncome}
            value={budget.actualIncome}
            height="h-4"
        />
    </div>
{/await}
