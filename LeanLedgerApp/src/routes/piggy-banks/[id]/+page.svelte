<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import Money from "$lib/components/Money.svelte";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import { page } from "$app/stores";
    import PiggyForm from "$lib/piggybanks/PiggyForm.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import { Fa } from "svelte-fa";
    import { faSave } from "@fortawesome/free-solid-svg-icons/faSave";
    import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
    import { goto } from "$app/navigation";
    import type { PiggyBank, PiggyBankWithEntries } from "$lib/piggybanks";
    import { encodeCategory, getCategoryName } from "$lib";

    let id = $page.params.id;
    let piggy = $state<PiggyBankWithEntries>();
    let isSaving = $state(false);

    async function load() {
        const resp = await apiClient.get<PiggyBankWithEntries>(
            `piggy-banks/${id}?${monthManager.params}`,
        );
        piggy = resp.data;
    }

    // allocations are returned from the API on piggy.allocations — use directly in the template

    async function saveChanges() {
        if (!piggy) return;
        isSaving = true;

        await apiClient.put(`piggy-banks/${id}`, piggy);
        await load();
        isSaving = false;
    }

    async function closePiggy() {
        await apiClient.delete(`piggy-banks/${id}`);
        await goto("/piggy-banks");

        return false;
    }
</script>

{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    {#if !piggy}
        <div class="p-4 variant-filled-error">Could not load piggy</div>
    {:else}
        <div class="mb-4 flex gap-4 justify-start items-center flex-wrap">
            <h1 class="h1">Piggy Bank</h1>
            <button class="btn text-primary-500 p-2" onclick={saveChanges}>
                <Fa icon={faSave} />
            </button>
            <DeleteConfirmationButton onDelete={closePiggy} />
            <h4 class="h4">
                Balance: <Money amount={piggy.balance ?? 0} />
            </h4>
            <h4 class="h4">
                Progress: {piggy.progress ?? "-"}
            </h4>

            {#if isSaving}
                <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
            {/if}
        </div>

        <PiggyForm bind:piggy />

        <div class="mt-8">
            <h2 class="h2 mb-4">Entries</h2>

            <div class="table-container">
                <table class="table table-compact table-hover w-full">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Amount</th>
                            <th>Description</th>
                            <th>Transaction</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- TODO: make these entries invidividually editable. Probably through a dialog -->
                        {#each piggy.entries as entry}
                            <tr>
                                <td>
                                    {entry.date}
                                </td>
                                <td><Money amount={entry.amount} /></td>
                                <td> {entry.description}</td>
                                <td>
                                    {#if entry.transaction}
                                        <a
                                            class="text-primary-400"
                                            href="/transactions/{entry
                                                .transaction.id}"
                                        >
                                            {entry.transaction.description}
                                        </a>
                                    {:else}
                                        -
                                    {/if}
                                </td>
                                <td>
                                    <button class="btn btn-sm"> Edit </button>
                                </td></tr
                            >
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {/if}
{:catch err}
    <div class="p-4 variant-filled-error">
        <h3 class="h3">Could not load piggy</h3>
        <p class="p">{String(err)}</p>
    </div>
{/await}
