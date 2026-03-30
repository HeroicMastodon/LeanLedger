<script lang="ts">
    import { apiClient } from "$lib/apiClient";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import Money from "$lib/components/Money.svelte";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import { page } from "$app/stores";
    import { apiClient as client } from "$lib/apiClient";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import MoneyInput from "$lib/components/forms/MoneyInput.svelte";
    import PiggyForm from "$lib/piggybanks/PiggyForm.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import { Fa } from "svelte-fa";
    import { faSave } from "@fortawesome/free-solid-svg-icons/faSave";
    import { goto } from "$app/navigation";
    import type { PiggyWithAllocationData } from "$lib/piggybanks";
    import { encodeCategory, getCategoryName } from "$lib";

    let id = $page.params.id;
    let piggy = $state<PiggyWithAllocationData>();
    let isSaving = $state(false);
    let searchDescription = $state("");
    let searchStartDate: string = $state("");
    let searchEndDate: string = $state("");
    let searchMinAmount = $state<number>();
    let searchMaxAmount = $state<number>();
    let searchResults: any[] = $state([]);
    let searchError: string = $state("");

    async function load() {
        const resp = await apiClient.get<PiggyWithAllocationData>(
            `piggy-banks/${id}?${monthManager.params}`,
        );
        piggy = resp.data;
    }

    // allocations are returned from the API on piggy.allocations — use directly in the template

    async function saveChanges() {
        if (!piggy) return;
        isSaving = true;

        const payload = {
            name: piggy.name,
            initialBalance: Number(piggy.initialBalance) || 0,
            balanceTarget: piggy.balanceTarget,
        };

        await apiClient.put(`piggy-banks/${id}`, payload);
        await load();
        isSaving = false;
    }

    async function closePiggy() {
        await apiClient.delete(`piggy-banks/${id}`);
        await goto("/piggy-banks");

        return false;
    }

    async function searchTransactions() {
        searchError = "";
        const desc = (searchDescription || "").trim();
        if (desc && desc.length < 3) {
            searchError =
                "Description must be at least 3 characters when provided.";
            return;
        }

        const params = new URLSearchParams();
        if (desc) params.set("description", desc);
        if (searchStartDate) params.set("startDate", searchStartDate);
        if (searchEndDate) params.set("endDate", searchEndDate);
        if (searchMinAmount !== null && searchMinAmount !== undefined)
            params.set("minAmount", String(searchMinAmount));
        if (searchMaxAmount !== null && searchMaxAmount !== undefined)
            params.set("maxAmount", String(searchMaxAmount));

        const resp = await client.get(
            `transactions/search?${params.toString()}`,
        );
        searchResults = resp.data;
    }

    async function createAllocationFromResult(txId: string, amount: number) {
        await client.post(`transactions/${txId}/allocations`, {
            piggyBankId: id,
            amount,
        });
        await load();
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
            <p>Balance: <Money amount={piggy.balance ?? 0} /></p>
            {#if isSaving}
                <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
            {/if}
        </div>

        <PiggyForm bind:piggy />

        <div class="flex items-center justify-start gap-4 mt-8 mb-4">
            <h2 class="h2">Allocations</h2>
            <FormButton
                class="btn-icon-sm p-2 variant-outline-primary text-primary-500"
                text="Find Transactions"
                confirmText="Close"
                onConfirm={async () => true}
                icon={faPlus}
            >
                <div class="p-2">
                    <div
                        class="grid grid-cols-1 md:grid-cols-6 gap-4 items-end mb-4"
                    >
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
                            <button
                                class="btn text-primary-500"
                                onclick={searchTransactions}>Search</button
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
                            <table
                                class="table table-compact table-hover w-full"
                            >
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
                                            <td
                                                ><Money
                                                    amount={searchResult.amount}
                                                /></td
                                            >
                                            <td>{searchResult.date}</td>
                                            <td class="flex gap-2 items-center">
                                                <MoneyInput
                                                    bind:value={
                                                        searchResult.__allocAmount
                                                    }
                                                />
                                                <button
                                                    class="btn text-success-500"
                                                    onclick={() =>
                                                        createAllocationFromResult(
                                                            searchResult.id,
                                                            Number(
                                                                searchResult.__allocAmount ??
                                                                    searchResult.amount,
                                                            ),
                                                        )}>Add</button
                                                >
                                            </td>
                                        </tr>
                                    {/each}
                                </tbody>
                            </table>
                        </div>
                    {/if}
                </div>
            </FormButton>
        </div>

        <div class="table-container">
            <table class="table table-compact table-hover w-full">
                <thead>
                    <tr>
                        <th>Description</th>
                        <th>Transaction Amount</th>
                        <th>Allocated</th>
                        <th>Source Account</th>
                        <th>Category</th>
                    </tr>
                </thead>
                <tbody>
                    {#each piggy.allocations as allocation}
                        <tr>
                            <td>
                                <a
                                    class="text-primary-400"
                                    href="/transactions/{allocation.transactionId}"
                                    >{allocation.description}</a
                                >
                            </td>
                            <td
                                ><Money
                                    amount={allocation.transactionAmount}
                                /></td
                            >
                            <td><Money amount={allocation.amount} /></td>
                            <td>
                                <a
                                    class="text-primary-400"
                                    href="/accounts/{allocation.sourceAccountId}"
                                    >{allocation.sourceAccountName}</a
                                >
                            </td>
                            <td>
                                <a
                                    class="text-primary-400"
                                    href="/categories/{encodeCategory(
                                        allocation.category,
                                    )}"
                                >
                                    {getCategoryName(allocation.category)}
                                </a>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
        <hr class="hr my-6" />
    {/if}
{:catch err}
    <div class="p-4 variant-filled-error">
        <h3 class="h3">Could not load piggy</h3>
        <p class="p">{String(err)}</p>
    </div>
{/await}
