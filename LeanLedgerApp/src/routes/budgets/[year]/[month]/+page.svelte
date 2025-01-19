<script lang="ts">
    import {page} from "$app/stores";
    import {monthFromNumber, lastMonth as getLastMonth, nextMonth as getNextMonth} from "$lib/dateTools";
    import {Fa} from "svelte-fa";
    import {faArrowLeft} from "@fortawesome/free-solid-svg-icons/faArrowLeft";
    import {faArrowRight} from "@fortawesome/free-solid-svg-icons/faArrowRight";

    const month = $derived(monthFromNumber($page.params.month, $page.params.year))
    const lastMonth = $derived(getLastMonth(month));
    const nextMonth = $derived(getNextMonth(month));
</script>

<div class="flex items-center gap-4">
    <a href="/budgets/{lastMonth.year}/{lastMonth.number}" class="btn btn-icon variant-outline-tertiary text-tertiary-500"><Fa icon={faArrowLeft}/></a>
    <h1 class="h1">Budget for {month.name} {month.year}</h1>
    <a href="/budgets/{nextMonth.year}/{nextMonth.number}" class="btn btn-icon variant-outline-tertiary text-tertiary-500"><Fa icon={faArrowRight}/></a>
</div>
