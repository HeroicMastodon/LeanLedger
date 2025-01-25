<script lang="ts">

    import DialogButton from "$lib/components/dialog/DialogButton.svelte";
    import {apiClient} from "$lib/apiClient";
    import type {MaybePromise} from "$lib";

    type ImportResult = "Success" | "Warning" | "Failed";
    type ImportedTransaction = {
        result: ImportResult,
        index: number,
        message: string,
        transactionId?: string
    };
    type ImportResponse = ImportedTransaction[];

    const props: { accountId: string; onClose: () => MaybePromise<any>; } = $props();
    let file = $state<Blob>();

    function setFile(e: Event & { currentTarget: EventTarget & HTMLInputElement }) {
        if (!e.currentTarget.files) return;

        file = e.currentTarget.files[0]
    }

    let importedTransactions: ImportResponse = $state([]);

    async function runImport() {
        importedTransactions = [];
        if (!file) {
            return false;
        }

        let formData = new FormData();
        formData.append("csvFile", file, "csvFile");
        const res = await apiClient.postForm<ImportResponse>(`imports/${props.accountId}`, formData);
        importedTransactions = res.data;

        return false;
    }
</script>

<DialogButton
    class="variant-outline-warning hidden lg:inline-block"
    text="Import"
    title="Import Account"
    confirmText="Import"
    cancelButtonColorType="error"
    cancelText="Close"
    confirmButtonColorType="warning"
    onConfirm={runImport}
    oncancel={props.onClose}
>
    <input name="csvFile" class="input" type="file" onchange={setFile} />
    <div class="max-h-96 max-w-7xl overflow-x-auto overflow-y-auto">
        {#each importedTransactions as transaction}
            <p class="p text-nowrap">
                {transaction.index}:
                <span
                    class:text-success-500={transaction.result === "Success"}
                    class:text-warning-400={transaction.result === "Warning"}
                    class:text-error-400={transaction.result === "Failed"}
                >
                {transaction.result};
            </span>
                {transaction.message}
                {#if transaction.transactionId}
                    <a
                        href="/transactions/{transaction.transactionId}"
                        target="_blank"
                        class="text-primary-400"
                    >
                        See Transaction
                    </a>
                {/if}
            </p>
        {/each}
    </div>
</DialogButton>
