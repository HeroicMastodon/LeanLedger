<script lang="ts">
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import {defaultTransaction, type Transaction} from "$lib/transactions";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import DefaultDialog from "$lib/components/DefaultDialog.svelte";
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";
    import {goto} from "$app/navigation";

    let transactions: Transaction[] = $state([]);

    async function load() {
        const response = await apiClient.get<{ transactions: Transaction[] }>("transactions");
        transactions = response.data.transactions;
    }

    let transactionDialog: HTMLDialogElement | undefined = $state();
    let transaction = $state(defaultTransaction());
    async function saveTransaction() {
        const resp = await apiClient.post("transactions", transaction);
        transactionDialog?.close();
        transaction = defaultTransaction();
        await load();
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
