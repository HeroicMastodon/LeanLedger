<script lang="ts">

    import DialogButton from "$lib/components/dialog/DialogButton.svelte";
    import {apiClient} from "$lib/apiClient";

    const props = $props<{ accountId: string; }>();
    let file = $state<Blob>();

    function setFile(e: Event & { currentTarget: EventTarget & HTMLInputElement }) {
        if (!e.currentTarget.files) return;

        file = e.currentTarget.files[0]
    }

    async function runImport() {
        if (!file) {
            return false;
        }

        let formData = new FormData();
        formData.append("csvFile", file, "csvFile");
        const res = await apiClient.postForm(`imports/${props.accountId}`, formData);

        return true;
    }

    async function close() {
    }
</script>

<DialogButton
    class="variant-outline-warning"
    text="Import"
    title="Import Account"
    confirmText="Import"
    cancelButtonColorType="error"
    confirmButtonColorType="warning"
    onConfirm={runImport}
>
    <input name="csvFile" class="input" type="file" onchange={setFile} />
</DialogButton>
