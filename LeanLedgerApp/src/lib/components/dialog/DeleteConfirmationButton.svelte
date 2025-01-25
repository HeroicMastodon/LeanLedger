<script lang="ts">

import DialogButton from "$lib/components/dialog/DialogButton.svelte";
import type {MaybePromise} from "$lib";
import {faTrashCan} from "@fortawesome/free-solid-svg-icons/faTrashCan";
let {onDelete, class: className}: {onDelete: () => MaybePromise<boolean>; class?: string;} = $props();
</script>

<DialogButton
    title="Are you sure you want to delete?"
    confirmText="Delete"
    onConfirm={onDelete}
    text="Delete"
    class="text-error-500 p-0 {className ?? ''}"
    confirmButtonColorType="error"
    cancelButtonColorType="success"
    icon={faTrashCan}
/>
