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

    let piggies: PiggyBank[] = $state([]);
    let newPiggy = $state(defaultPiggyBank());

    async function load() {
        const resp = await apiClient.get<PiggyBank[]>(
            `piggy-banks?${monthManager.params}`,
        );
        piggies = resp.data;
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
        <!-- TODO: Retrieve and display net worth here -->
        <div class="font-bold">
            Net Worth: <Money amount={0} />
        </div>
        <!-- TODO: Calulate and display difference here -->
        <div class="font-bold">
            Difference: <Money amount={0} />
        </div>
    </div>
    <div>
        <!-- TODO: If difference is > 0, show we need to allocate
                if difference is < 0, show we need to add disbursements
        -->
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
                    <th>Balance</th>
                    <th>Target</th>
                    <th>Progress</th>
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
                        <td><Money amount={piggy.balance} /></td>
                        <td>
                            {#if piggy.targetBalance != null}
                                <Money amount={piggy.targetBalance} />
                            {:else}
                                -
                            {/if}
                        </td>
                        <td class="text-left">
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
        <p class="p">{String(err)}</p>
    </div>
{/await}
