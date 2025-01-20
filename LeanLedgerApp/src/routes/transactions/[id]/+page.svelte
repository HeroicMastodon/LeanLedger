<script lang="ts">
    import {
        defaultTransaction, type EditableTransaction, loadAccountOptions, loadCategoryOptions,
    } from "$lib/transactions";
    import {onMount} from "svelte";
    import {apiClient} from "$lib/apiClient";
    import {ProgressRadial} from "@skeletonlabs/skeleton";
    import {goto} from "$app/navigation"
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import {page} from "$app/stores";

    let id: string = $page.params.id;
    let transaction: EditableTransaction = $state(defaultTransaction());
    onMount(async () => {
        const res = await apiClient.get<EditableTransaction>(`transactions/${id}`)
        transaction = res.data;
    });

    async function deleteTransaction() {
        const resp = await apiClient.delete(`transactions/${id}`);
        if (resp.status == 204) {
            await goto("/transactions");
        }

        return false;
    }

    let isSaving = $state(false);

    async function saveTransaction() {
        isSaving = true;
        const resp = await apiClient.put(`transactions/${id}`, transaction);
        isSaving = false;
    }
</script>
<div class="flex gap-4 mb-8 justify-start items-center">
    <h1 class="h1">Transaction</h1>
    <button
            onclick={saveTransaction}
            class="btn variant-filled-primary"
    >Save
    </button>
    <DeleteConfirmationButton onDelete={deleteTransaction} />
    {#if isSaving}
        <ProgressRadial width="w-5" meter="stroke-primary-500" track="strock-primary-500/30" />
    {/if}
</div>

<TransactionForm bind:transaction />
