<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import Money from "$lib/components/Money.svelte";
    import { Dialog } from "$lib/dialog.svelte";
    import { type PiggyBankEntry } from "$lib/piggybanks";
    import Fa from "svelte-fa";
    import PiggyBankEntryForm from "./PiggyBankEntryForm.svelte";
    import { faEdit } from "@fortawesome/free-solid-svg-icons/faEdit";

    let {
        entries,
        showPiggyBank,
    }: { entries: PiggyBankEntry[]; showPiggyBank?: boolean } = $props();

    let selectedEntry = $state<PiggyBankEntry>();
    let dialog = new Dialog();

    function selectEntry(entry: PiggyBankEntry) {
        selectedEntry = entry;
        dialog.open();
    }
    async function saveEntry() {
        if (!selectedEntry) return true;
        await apiClient.put(
            `piggy-banks/entries/${selectedEntry.id}`,
            selectedEntry,
        );

        return true;
    }
</script>

<div class="table-container">
    <table class="table table-compact table-hover w-full">
        <thead>
            <tr>
                {#if showPiggyBank}
                    <th>Piggy Bank</th>
                {/if}
                <th>Date</th>
                <th>Amount</th>
                <th>Description</th>
                <th>Transaction</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            {#each entries as entry}
                <tr>
                    {#if showPiggyBank}
                        <td>{entry.piggyBank.name}</td>
                    {/if}
                    <td>
                        {entry.date}
                    </td>
                    <td><Money amount={entry.amount} /></td>
                    <td> {entry.description}</td>
                    <td>
                        {#if entry.transaction}
                            <a
                                class="text-primary-400"
                                href="/transactions/{entry.transaction.id}"
                            >
                                {entry.transaction.description}
                            </a>
                        {:else}
                            -
                        {/if}
                    </td>
                    <td>
                        <button
                            onclick={() => selectEntry(entry)}
                            class="btn btn-icon-sm"
                        >
                            <Fa icon={faEdit} />
                        </button>
                    </td>
                </tr>
            {/each}
        </tbody>
    </table>
</div>
<DefaultDialog bind:dialog={dialog.value} onenter={saveEntry}>
    {#if selectedEntry}
        <div class="flex flex-col gap-4">
            <PiggyBankEntryForm bind:entry={selectedEntry} />
            <div class="flex gap-4 items-center justify-end">
                <button onclick={() => dialog.close()} class="btn variant-outline-error">Cancel</button>
                <button onclick={saveEntry} class="btn variant-filled-success">Save</button>
            </div>
        </div>
    {:else}
        <div class="p-4 variant-filled-error">No entry selected</div>
    {/if}
</DefaultDialog>
