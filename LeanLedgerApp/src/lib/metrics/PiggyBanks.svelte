<script lang="ts">
    import Card from "$lib/components/Card.svelte";
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import Money from "$lib/components/Money.svelte";
    import ProgressPercent from "$lib/components/ProgressPercent.svelte";
    import PiggyBankDiffText from "$lib/piggybanks/PiggyBankDiffText.svelte";

    type PiggyBankMetric = {
        id: string;
        name: string;
        targetBalance?: number;
        balance: number;
        progress?: number;
        change: number;
    };

    type PiggyMetricsResponse = {
        piggyBanks: PiggyBankMetric[];
        piggyTotal: number;
        accountTotal: number;
    };

    const metricsPromise = $derived.by(async () => {
        const resp = await apiClient.get<PiggyMetricsResponse>(
            `metrics/piggybanks?${monthManager.params}`,
        );
        return resp.data;
    });

    function difference(metrics: PiggyMetricsResponse) {
        return metrics.accountTotal - metrics.piggyTotal;
    }
</script>

<!-- TODO: Handle loading differently so I can use reactivity correctly -->
{#await metricsPromise}
    <Card class="grow lg:flex-initial">
        <h2 class="h2">Piggy Banks</h2>
        <hr class="hr" />
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    </Card>
{:then metrics}
    <Card class="grow lg:flex-initial">
        <h2 class="h2">Piggy Banks</h2>
        <hr class="hr mb-4" />

        <table class="w-full">
            <thead>
                <tr>
                    <th></th>
                    <th class="text-left px-1">Balance</th>
                    <th class="text-center px-1">Change</th>
                    <th class="text-center px-1">Target</th>
                    <th class="text-right">Progress</th>
                </tr>
            </thead>
            <tbody>
                {#each metrics.piggyBanks as piggy}
                    <tr>
                        <td>{piggy.name}</td>
                        <td class="text-center"><Money amount={piggy.balance} /></td>
                        <td class="text-center"><Money amount={piggy.change} /></td>
                        <td class="text-center">
                            <Money amount={piggy.targetBalance ?? 0} />
                        </td>
                        <td class="text-right">
                            <ProgressPercent progress={piggy.progress} />
                        </td>
                    </tr>
                {/each}
            </tbody>
        </table>

        <hr class="hr mt-4" />
        <div class="flex items-center justify-between gap-4">
            <div>
                Total in Piggies: <Money amount={metrics.piggyTotal} />
            </div>
            <div class="text-sm">
                <PiggyBankDiffText difference={difference(metrics)}  />
            </div>
        </div>
    </Card>
{:catch _err}
    <Card class="variant-filled-error">
        <h2 class="h2">Piggy Banks</h2>
        <hr class="hr" />
        <p class="p">Could not retrieve piggy metrics</p>
    </Card>
{/await}
