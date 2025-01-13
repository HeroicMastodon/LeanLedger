<script lang="ts">

import FormButton from "$lib/components/dialog/FormButton.svelte";
import Alert from "$lib/components/Alert.svelte";
import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
import {ProgressBar} from "@skeletonlabs/skeleton";
import {todaysDateString, firstOfThisYear} from "$lib/dateTools";

let {run, countPromise, text}: {
    run: (start: string, end: string) => void;
    text: string;
    countPromise: Promise<number | undefined>
} = $props();

let startDate = $state(firstOfThisYear());
let endDate = $state(todaysDateString());
</script>

<FormButton
    class="variant-outline-warning"
    text={text}
    confirmText="Run"
    onConfirm={() => {run(startDate, endDate); return false;}}
>
    <div class="flex gap-8">
        <LabeledInput
            type="date"
            bind:value={startDate}
            label="start"
        />
        <LabeledInput
            type="date"
            bind:value={endDate}
            label="end"
        />
    </div>
    {#await countPromise}
        <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
    {:then count}
        <Alert show={count !== undefined} class="variant-filled-success"><p>{count} transactions were edited by the rule</p></Alert>
    {:catch err}
        <Alert show class="variant-filled-error"><p>{err}</p></Alert>
    {/await}
</FormButton>
