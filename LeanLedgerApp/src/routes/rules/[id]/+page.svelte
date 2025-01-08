<script lang="ts">

    import type {Rule} from "$lib/rules";
    import Alert from "$lib/components/Alert.svelte";
    import {ProgressBar, ProgressRadial} from "@skeletonlabs/skeleton";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";

    const {data}: { data: PageData; } = $props();
    let rule = $state<Rule | undefined>();
    let loading = $state(load());

    async function load() {
        const res = await apiClient.get<Rule>(`Rules/${data.id}`);
        rule = res.data;
    }
</script>

<div class="mb-8 flex">
    <h1 class="h1">Rule</h1>
</div>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    {#if rule}
        <!--TODO: make a card component-->
        <div class="card p-8">
            <h2 class="h2">{rule.name}</h2>
        </div>
    {:else}
       <Alert class="variant-filled-error"><p>The Api was invoked successfully but no rule was returned</p></Alert>
    {/if}
{:catch err}
    <Alert class="variant-filled-error">
        <p>Could not load rule:</p>
        <p>{err}</p>
    </Alert>
{/await}
