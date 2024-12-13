<script lang="ts">
    import {
        type AttachedAccount,
        defaultTransaction, type EditableTransaction, loadAccountOptions, loadCategoryOptions,
        type Transaction,
        TransactionTypeOptions
    } from "$lib/transactions";
    import {onMount} from "svelte";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";
    import {type PopupSettings, popup, ProgressRadial} from "@skeletonlabs/skeleton";
    import DefaultDialog from "$lib/components/DefaultDialog.svelte";
    import {goto} from "$app/navigation"
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";

    const popupSettings: PopupSettings = {
        event: 'focus-click',
        target: 'select',
        placement: 'bottom',
    };

    const {data}: { data: PageData; } = $props();
    let transaction: EditableTransaction = $state(defaultTransaction());
    onMount(async () => {
        const res = await apiClient.get<EditableTransaction>(`transactions/${data.id}`)
        transaction = res.data;
    });

    let deleteConfirmationDialog: HTMLDialogElement | undefined = $state();
    async function deleteTransaction() {
        const resp = await apiClient.delete(`transactions/${data.id}`);
        if (resp.status == 204) {
            await goto("/transactions");
        }
    }

    let isSaving = $state(false);
    async function saveTransaction() {
        isSaving = true;
        const resp = await apiClient.put(`transactions/${data.id}`, transaction);
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
    <button
        onclick={() => deleteConfirmationDialog?.showModal()}
        class="btn variant-outline-error"
    >Delete
    </button>
    <DefaultDialog bind:dialog={deleteConfirmationDialog}>
        <div class="flex flex-col gap-4">
            <h2 class="h2">Are you sure you want to delete?</h2>
            <div class="flex gap-4">
                <button
                    onclick={() => deleteConfirmationDialog?.close()}
                    class="btn variant-outline-success"
                >Cancel
                </button>
                <button
                    onclick={deleteTransaction}
                    class="btn variant-filled-error"
                >
                    Delete
                </button>
            </div>
        </div>
    </DefaultDialog>
    {#if isSaving}
        <ProgressRadial width="w-5" meter="stroke-primary-500" track="strock-primary-500/30" />
    {/if}
</div>

<TransactionForm bind:transaction/>
