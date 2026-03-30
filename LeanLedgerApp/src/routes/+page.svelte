<script>
    import NetWorth from "$lib/metrics/NetWorth.svelte";
    import BudgetMetrics from "$lib/metrics/BudgetMetrics.svelte";
    import AccountTrends from "$lib/metrics/AccountTrends.svelte";
    import PiggyBanks from "$lib/metrics/PiggyBanks.svelte";
</script>

<div class="container h-full flex justify-start items-start gap-4 flex-wrap">
    <NetWorth />
    <AccountTrends />
    <BudgetMetrics />
    <PiggyBanks />
</div>
