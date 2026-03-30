<script lang="ts">
    import Card from "$lib/components/Card.svelte";
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import Money from "$lib/components/Money.svelte";

    type Piggy = {
        id: string;
        name: string;
        initialBalance: number;
        balanceTarget?: number;
        balance: number;
        progressPercent: number | null;
        closed: boolean;
    };

    type PiggyMetricsResponse = {
        piggyBanks: Piggy[];
        totals: {
            piggyTotal: number;
            accountTotal: number;
            piggyTotalExceedsAccounts: boolean;
        };
    };

    const metricsPromise = $derived.by(async () => {
        const resp = await apiClient.get<PiggyMetricsResponse>(
            `metrics/piggy-banks?${monthManager.params}`,
        );
        return resp.data;
    });
</script>

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

        <table class="w-96">
            <thead>
                <tr>
                    <th></th>
                    <th class="text-left">Balance</th>
                    <th class="text-left">Target</th>
                    <th class="text-left">Progress</th>
                </tr>
            </thead>
            <tbody>
                {#each metrics.piggyBanks as p}
                    <tr>
                        <td>{p.name}</td>
                        <td class="text-left"><Money amount={p.balance} /></td>
                        <td class="text-left">
                            <Money amount={p.balanceTarget ?? 0} />
                        </td>
                        <td
                            class="text-left"
                            class:text-error-500={(p.progressPercent ?? 0) < 40}
                            class:text-warning-500={(p.progressPercent ?? 0) <
                                90 && (p.progressPercent ?? 0) >= 40}
                            class:text-success-500={(p.progressPercent ?? 0) >=
                                90}
                            >{p.progressPercent
                                ? `${p.progressPercent.toFixed(1)}%`
                                : "-"}</td
                        >
                    </tr>
                {/each}
            </tbody>
        </table>

        <hr class="hr mt-4" />
        <div class="flex items-center justify-between gap-4">
            <div>
                Total in Piggies: <Money amount={metrics.totals.piggyTotal} />
            </div>
            <div class="text-sm">
                Accounts: <Money amount={metrics.totals.accountTotal} />
                {#if metrics.totals.piggyTotalExceedsAccounts}
                    <span class="ml-2 text-warning-400"
                        >(Piggy total exceeds accounts)</span
                    >
                {/if}
            </div>
        </div>
    </Card>
{:catch err}
    <Card class="variant-filled-error">
        <h2 class="h2">Piggy Banks</h2>
        <hr class="hr" />
        <p class="p">Could not retrieve piggy metrics</p>
    </Card>
{/await}
