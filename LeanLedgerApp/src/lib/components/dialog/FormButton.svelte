<script lang="ts">

    import DialogButton from "$lib/components/dialog/DialogButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import type {MaybePromise} from "$lib";

    let {children, onConfirm, error = $bindable(), text, confirmText}: {
        onConfirm: () => MaybePromise<boolean>;
        children: any,
        error?: string;
        text: string;
        confirmText?: string;
    } = $props();
</script>

<DialogButton
    class="variant-filled-primary"
    title={text}
    text={text}
    onConfirm={onConfirm}
    confirmText={confirmText ?? "Save"}
    confirmButtonColorType="success"
    cancelButtonColorType="error"
>
    {@render children()}
    <Alert show={!!error} class="variant-filled-error">
        <p>{error}</p>
    </Alert>
</DialogButton>
