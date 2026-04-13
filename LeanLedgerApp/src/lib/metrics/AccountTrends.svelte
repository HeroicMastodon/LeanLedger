<script lang="ts">
    import Card from "$lib/components/Card.svelte";
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { ProgressBar, TabGroup, Tab } from "@skeletonlabs/skeleton";
    import Money from "$lib/components/Money.svelte";

    type Trend = {
        label: string | number | Date;
        netWorth: number;
        change: number;
        totalChange: number;
    };

    type TrendType = "yearly" | "monthly" | "quarterly";
    type AccountTrends = Record<TrendType, Trend[]>;

    let loading = $state(true);
    let trends = $state<AccountTrends>({
        yearly: [],
        monthly: [],
        quarterly: [],
    });
    let params = $derived(monthManager.params);

    async function load() {
        const resp = await apiClient.get<AccountTrends>(
            `Metrics/account-trends?${params}`,
        );

        trends = resp.data;
        loading = false;
    }

    $effect(() => {
        load();
    });

    let tabSet = $state<TrendType>("monthly");
    let currentTrend = $derived(trends[tabSet]);
</script>

<Card class="grow lg:flex-initial">
    <h2 class="h2">Account Trends</h2>
    <hr class="hr" />
    {#if loading}
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    {:else}
        <TabGroup>
            <Tab bind:group={tabSet} name="Monthly" value={"monthly"}>
                Monthly
            </Tab>
            <Tab bind:group={tabSet} name="Quarterly" value={"quarterly"}>
                Quarterly
            </Tab>
            <Tab bind:group={tabSet} name="Yearly" value={"yearly"}>Yearly</Tab>
            <!-- Tab Panels --->

            <svelte:fragment slot="panel">
                <table class="w-96">
                    <thead>
                        <tr>
                            <th></th>
                            <th class="text-left">Net Worth</th>
                            <th class="text-left">Change</th>
                            <th class="text-left">Total Change</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each currentTrend as trend}
                            <tr>
                                <td>{trend.label}</td>
                                <td>
                                    <Money amount={trend.netWorth} />
                                </td>
                                <td>
                                    <Money amount={trend.change} />
                                </td>
                                <td>
                                    <Money amount={trend.totalChange} />
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </svelte:fragment>
        </TabGroup>
    {/if}
</Card>
