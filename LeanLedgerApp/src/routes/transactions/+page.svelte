<script lang="ts">
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import {defaultTransaction, type EditableTransaction, type Transaction} from "$lib/transactions";
    import {apiClient} from "$lib/apiClient";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";
    import type {AxiosError} from "axios";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import Alert from "$lib/components/Alert.svelte";

    let transactions: Transaction[] = $state([]);

    async function load() {
        const response = await apiClient.get<Transaction[]>("transactions");
        transactions = response.data;
    }

    let transaction = $state(defaultTransaction());
    let error: string | undefined = $state();

    async function saveTransaction() {
        try {
            error = undefined;
            const resp = await apiClient.post<EditableTransaction>("transactions", transaction);
            transaction = defaultTransaction();
            await load();
        } catch (e) {
            const err = e as AxiosError<{ detail: string; }>;
            console.dir(err.response?.data.detail);
            error = err.response?.data.detail;
            return false;
        }

        return true;
    }
</script>
<div class="mb-8 flex justify-start items-center gap-8">
    <h1 class="h1">Transactions</h1>
    <FormButton
            text="New Transaction"
            bind:error
            onConfirm={saveTransaction}
            confirmText="Create"
    >
        <TransactionForm bind:transaction />
    </FormButton>
</div>
<TransactionTable transactions={transactions} />
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:catch err}
    <Alert show>
        <h3 class="h3">Something went wrong</h3>
        <p>We couldn't load your transactions. Please try again </p>
        <p>{!!err ? err : ""}</p>
    </Alert>
{/await}
