<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import Money from "$lib/components/Money.svelte";
    import PiggyForm from "$lib/piggybanks/PiggyForm.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import { defaultPiggyBank, type PiggyBank } from "$lib/piggybanks";
    import ProgressPercent from "$lib/components/ProgressPercent.svelte";
    import PiggyBankDiffText from "$lib/piggybanks/PiggyBankDiffText.svelte";

    let piggies: PiggyBank[] = $state([]);
    let newPiggy = $state(defaultPiggyBank());
    let netWorth = $state(0);

    async function load() {
        const resp = await apiClient.get<PiggyBank[]>(
            `piggy-banks?${monthManager.params}`,
        );
        piggies = resp.data;

        const netWorthResp = await apiClient.get<{ netWorth: number }>(
            `metrics/net-worth/amount?${monthManager.params}`,
        );
        netWorth = netWorthResp.data.netWorth;
    }

    async function saveNewPiggy() {
        await apiClient.post("piggy-banks", newPiggy);
        newPiggy = defaultPiggyBank();
        await load();
        return true;
    }

    const totalBalance = $derived(
        piggies.reduce((sum, p) => sum + (p.balance ?? 0), 0),
    );
    const difference = $derived(netWorth - totalBalance);
</script>

<div class="mb-8 flex gap-4 items-center">
    <h1 class="h1">Piggy Banks</h1>
    <FormButton
        class="btn-icon-sm p-2 variant-outline-primary text-primary-500"
        text="New Piggy Bank"
        onConfirm={saveNewPiggy}
        confirmText="Create"
        icon={faPlus}
    >
        <PiggyForm bind:piggy={newPiggy} />
    </FormButton>
    <div>
        <div class="font-bold">
            Total Balance: <Money amount={totalBalance} />
        </div>
        <div class="font-bold">
            Net Worth: <Money amount={netWorth} />
        </div>
        <div class="font-bold">
            <PiggyBankDiffText {difference} />
        </div>
    </div>
</div>

{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <div class="table-container">
        <table class="table table-compact table-hover md:table-fixed">
            <thead>
                <tr>
                    <th>Name</th>
                    <th class="text-center">Balance</th>
                    <th class="text-center">Target</th>
                    <th class="text-right">Progress</th>
                </tr>
            </thead>
            <tbody>
                {#each piggies as piggy}
                    <tr>
                        <td
                            ><a
                                class="text-primary-400"
                                href="/piggy-banks/{piggy.id}">{piggy.name}</a
                            ></td
                        >
                        <td class="text-center"><Money amount={piggy.balance} /></td>
                        <td class="text-center">
                            {#if piggy.targetBalance != null}
                                <Money amount={piggy.targetBalance} />
                            {:else}
                                -
                            {/if}
                        </td>
                        <td class="text-right">
                            <ProgressPercent progress={piggy.progress} />
                        </td>
                    </tr>
                {/each}
            </tbody>
        </table>
    </div>
{:catch err}
    <div class="p-4 variant-filled-error">
        <h3 class="h3">Could not load piggy banks</h3>
        <p class="p">{err}</p>
    </div>
{/await}
