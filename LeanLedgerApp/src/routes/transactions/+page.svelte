<script lang="ts">
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import {defaultTransaction, type EditableTransaction, type Transaction} from "$lib/transactions";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import DefaultDialog from "$lib/components/DefaultDialog.svelte";
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";
    import {goto} from "$app/navigation";
    import type {AxiosError} from "axios";

    let transactions: Transaction[] = $state([]);

    async function load() {
        const response = await apiClient.get<Transaction[]>("transactions");
        transactions = response.data;
    }

    let transactionDialog: HTMLDialogElement | undefined = $state();
    let transaction = $state(defaultTransaction());
    let error: string | undefined = $state();

    async function saveTransaction() {
        try {
            error = undefined;
            const resp = await apiClient.post<EditableTransaction>("transactions", transaction);
            transactionDialog?.close();
            transaction = defaultTransaction();
            await load();
        } catch (e) {
            const err = e as AxiosError<{ detail: string; }>;
            console.dir(err.response?.data.detail);
            error = err.response?.data.detail;
        }
    }
</script>
<div class="mb-8 flex justify-start items-center gap-8">
    <h1 class="h1">Transactions</h1>
    <button
            onclick={() => transactionDialog?.showModal()}
            class="btn variant-filled-primary"
    >New Transaction
    </button>
    <DefaultDialog bind:dialog={transactionDialog}>
        <div class="flex flex-col gap-4">
            <h2 class="h2">New Transaction</h2>
            <TransactionForm bind:transaction />
            <div class="flex flex-row gap-4 justify-center">
                <button
                        onclick={() => transactionDialog?.close()}
                        class="btn variant-outline-error"
                >Cancel
                </button>
                <button
                        onclick={saveTransaction}
                        class="btn variant-filled-success"
                >Save
                </button>
            </div>
            {#if error}
                <div class="alert variant-filled-error">
                    <div class="alert-message">
                        <p>{error}</p>
                    </div>
                </div>
            {/if}
        </div>
    </DefaultDialog>
</div>
<TransactionTable transactions={transactions} />
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:catch err}
    <div class="alert variant-filled-error">
        <div class="alert-message">
            <h3 class="h3">Something went wrong</h3>
            <p>We couldn't load your transactions. Please try again </p>
            <p>{!!err ? err : ""}</p>
        </div>
    </div>
{/await}
