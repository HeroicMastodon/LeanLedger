<script lang="ts">
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import Money from "$lib/components/Money.svelte";

    // TODO: simplify this and just use a transaction table in the results

    let searchDescription = $state("");
    let searchStartDate = $state("");
    let searchEndDate = $state("");
    let searchMinAmount = $state<number>();
    let searchMaxAmount = $state<number>();
    let searchResults: any[] = $state([]);
    let searchError = $state<string | null>(null);

    async function searchTransactions() {
        searchError = null;
        try {
            const response = await fetch("/api/transactions/search", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    description: searchDescription,
                    startDate: searchStartDate,
                    endDate: searchEndDate,
                    minAmount: searchMinAmount,
                    maxAmount: searchMaxAmount,
                }),
            });
            if (!response.ok) {
                throw new Error(
                    `Search failed: ${response.status} ${response.statusText}`,
                );
            }
            const data = await response.json();
            searchResults = data.transactions;
        } catch (error) {
            console.error("Error searching transactions:", error);
            searchError =
                error instanceof Error
                    ? error.message
                    : "An unknown error occurred during the search.";
        }
    }
</script>

<div class="p-2">
    <div class="grid grid-cols-1 md:grid-cols-6 gap-4 items-end mb-4">
        <LabeledInput
            label="Description"
            type="text"
            bind:value={searchDescription}
            class="md:col-span-2"
        />
        <LabeledInput
            label="Start Date"
            type="date"
            bind:value={searchStartDate}
            class="md:col-span-1"
        />
        <LabeledInput
            label="End Date"
            type="date"
            bind:value={searchEndDate}
            class="md:col-span-1"
        />
        <MoneyInput
            label="Min Amount"
            bind:value={searchMinAmount}
            class="md:col-span-1"
        />
        <MoneyInput
            label="Max Amount"
            bind:value={searchMaxAmount}
            class="md:col-span-1"
        />
        <div class="md:col-span-6">
            <button class="btn text-primary-500" onclick={searchTransactions}
                >Search</button
            >
        </div>
    </div>
    {#if searchError}
        <div class="p-2 variant-filled-warning mb-4">
            {searchError}
        </div>
    {/if}

    {#if searchResults.length > 0}
        <div class="table-container">
            <table class="table table-compact table-hover w-full">
                <thead>
                    <tr>
                        <th>Description</th>
                        <th>Amount</th>
                        <th>Date</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {#each searchResults as searchResult}
                        <tr>
                            <td
                                ><a
                                    class="text-primary-400"
                                    href="/transactions/{searchResult.id}"
                                    >{searchResult.description}</a
                                ></td
                            >
                            <td><Money amount={searchResult.amount} /></td>
                            <td>{searchResult.date}</td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
