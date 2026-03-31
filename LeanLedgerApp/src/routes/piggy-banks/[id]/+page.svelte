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
    import type { PiggyWithAllocationData } from "$lib/piggybanks";
    import { encodeCategory, getCategoryName } from "$lib";

    let id = $page.params.id;
    let piggy = $state<PiggyWithAllocationData>();
    let isSaving = $state(false);

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

        <div class="mt-8">
            <h2 class="h2 mb-4">Allocations</h2>

            <div class="table-container">
                <table class="table table-compact table-hover w-full">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Amount</th>
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
        </div>
        <hr class="hr my-6" />

        <div class="mt-8">
            <div class="flex gap-4 justify-start items-center">
                <h2 class="h2 mb-4">Disbursements</h2>
                <button class="btn btn-icon-sm p-2 variant-outline-primary text-primary-500">
                    <Fa icon={faPlus} />
                </button>
            </div>

            <div class="table-container">
                <table class="table table-compact table-hover w-full">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Amount</th>
                            <th>Transaction</th>
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
        </div>
    {/if}
{:catch err}
    <div class="p-4 variant-filled-error">
        <h3 class="h3">Could not load piggy</h3>
        <p class="p">{String(err)}</p>
    </div>
{/await}
