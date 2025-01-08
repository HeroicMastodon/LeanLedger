<script lang="ts">
    import {
        type Rule,
        type RuleActionType,
        ruleActionTypes,
        type RuleCondition,
        ruleConditions,
        ruleTransactionFields
    } from "$lib/rules";
    import Alert from "$lib/components/Alert.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import {splitPascal} from "$lib";

    const {data}: { data: PageData; } = $props();
    let rule = $state<Rule | undefined>();
    let loading = $state(load());
    let triggerValueOptions = $state<string[]>([]);
    let actionValueOptions = $state<string[]>([]);

    async function load() {
        const res = await apiClient.get<Rule>(`Rules/${data.id}`);
        rule = res.data;
    }

    function isTriggerValueDisabled(condition: RuleCondition) {
        return condition === "Exists";
    }

    function isActionValueDisabled(actionType: RuleActionType) {
        return actionType === "DeleteTransaction";
    }
</script>

<div class="mb-8 flex justify-between">
    <div class="flex gap-4">
        <h1 class="h1">Rule</h1>
        <button class="btn variant-filled-primary">Save</button>
        <button class="btn variant-outline-secondary">Matching Transactions</button>
        <button class="btn variant-outline-warning">Run Rule</button>
    </div>
    <button class="btn variant-outline-error">Delete</button>
</div>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    {#if rule}
        <div class="flex gap-8 items-end w-fit mb-8">
            <LabeledInput
                label="Name"
                bind:value={rule.name}
                type="text"
            />
            <label class="md:col-span-1 flex space-x-2 items-center">
                <input type="checkbox" class="checkbox mt-2 mb-2" bind:checked={rule.isStrict} />
                <span>Is Strict?</span>
            </label>
        </div>

        <div class="mb-4 flex gap-4">
            <h2 class="h2"> Rule triggers when </h2>
            <button class="btn variant-outline-primary">New Trigger</button>
        </div>
        <div class="table-container mb-8">
            <table class="table">
                <thead>
                <tr>
                    <th>Field</th>
                    <th>Not</th>
                    <th>Not Condition</th>
                    <th>Value</th>
                </tr>
                </thead>
                <tbody>
                {#each rule.triggers as trigger, idx}
                    <tr>
                        <td>
                            <button class="btn variant-outline-error mr-4">Delete</button>
                            <select class="select w-fit" bind:value={trigger.field}>
                                {#each ruleTransactionFields as field}
                                    <option value={field}>{splitPascal(field)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <input class="checkbox" type="checkbox" bind:checked={trigger.not} />
                        </td>
                        <td>
                            <select class="select w-fit" bind:value={trigger.condition}>
                                {#each ruleConditions as condition}
                                    <option value={condition}>{splitPascal(condition)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <input
                                class="input"
                                type="text"
                                list="trigger-value"
                                bind:value={trigger.value}
                                disabled={isTriggerValueDisabled(trigger.condition)}
                            />
                        </td>
                    </tr>
                {/each}
                </tbody>
            </table>
        </div>
        <datalist id="trigger-value">
            {#each triggerValueOptions as option}
                <option>{option}</option>
            {/each}
        </datalist>

        <div class="mb-4 flex gap-4">
            <h2 class="h2"> Then rule will </h2>
            <button class="btn variant-outline-primary">New Action</button>
        </div>
        <div class="table-container">
            <table class="table">
                <thead>
                <tr>
                    <th>Action</th>
                    <th>Field</th>
                    <th>Value</th>
                </tr>
                </thead>
                <tbody>
                {#each rule.actions as action}
                    <tr>
                        <td>
                            <button class="btn variant-outline-error mr-4">Delete</button>
                            <select class="select w-fit" bind:value={action.actionType}>
                                {#each ruleActionTypes as actionType}
                                    <option value={actionType}>{splitPascal(actionType)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <select class="select w-fit" bind:value={action.field}>
                                {#each ruleTransactionFields as field}
                                    <option value={field}>{splitPascal(field)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <input
                                class="input"
                                type="text"
                                list="action-value"
                                bind:value={action.value}
                                disabled={isActionValueDisabled(action.actionType)}
                            />
                        </td>
                    </tr>
                {/each}
                </tbody>
            </table>
        </div>
        <datalist id="action-value">
            {#each actionValueOptions as option}
                <option>{option}</option>
            {/each}
        </datalist>
    {:else}
        <Alert class="variant-filled-error"><p>The Api was invoked successfully but no rule was returned</p></Alert>
    {/if}
{:catch err}
    <Alert class="variant-filled-error">
        <p>Could not load rule:</p>
        <p>{err}</p>
    </Alert>
{/await}
