<script lang="ts">
    import Money from "$lib/components/Money.svelte";
    import { type PiggyBankEntry } from "$lib/piggybanks";

    let {
        entries,
        showPiggyBank,
    }: { entries: PiggyBankEntry[]; showPiggyBank?: boolean } = $props();
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
            <!-- TODO: make these entries invidividually editable. Probably through a dialog -->
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
                        <button class="btn btn-sm"> Edit </button>
                    </td>
                </tr>
            {/each}
        </tbody>
    </table>
</div>
