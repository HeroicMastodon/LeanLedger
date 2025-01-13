<script lang="ts">

import DialogButton from "$lib/components/dialog/DialogButton.svelte";
import type {MaybePromise} from "$lib";
let {onDelete}: {onDelete: () => MaybePromise<boolean>;} = $props();
</script>

<DialogButton
    title="Are you sure you want to delete?"
    confirmText="Delete"
    onConfirm={onDelete}
    text="Delete"
    class="variant-outline-error"
    confirmButtonColorType="error"
    cancelButtonColorType="success"
    onenter={onDelete}
/>
