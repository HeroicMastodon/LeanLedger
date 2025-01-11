<script lang="ts">
    import {
        defaultRuleAction,
        defaultRuleTrigger,
        type Rule,
        type RuleActionType,
        ruleActionTypes,
        type RuleCondition,
        ruleConditions,
        type RuleTransactionField,
        ruleTransactionFields, type RuleTrigger
    } from "$lib/rules";
    import Alert from "$lib/components/Alert.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {apiClient} from "$lib/apiClient";
    import type {PageData} from "./$types";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import {type SelectOption, splitPascal} from "$lib";
    import RuleValueInput from "$lib/rules/RuleValueInput.svelte";
    import {loadAccountOptions} from "$lib/transactions";

    const {data}: { data: PageData; } = $props();
    let rule = $state<Rule | undefined>();
    let loading = $state(load());
    let accounts = $state<SelectOption<string>[]>([]);

    async function load() {
        const ruleResponse = await apiClient.get<Rule>(`Rules/${data.id}`);
        rule = ruleResponse.data;

        accounts = await loadAccountOptions();
    }

    async function save() {
        const res = await apiClient.put<Rule>(`Rules/${data.id}`, rule);
        rule = res.data;
    }

    function addTrigger() {
        rule?.triggers.push(defaultRuleTrigger());
    }

    function removeTrigger(idx: number) {
        rule?.triggers.splice(idx, 1);
    }

    function addAction() {
        rule?.actions.push(defaultRuleAction());
    }

    function removeAction(idx: number) {
        rule?.actions.splice(idx, 1);
    }

    function isTriggerValueDisabled(condition: RuleCondition) {
        return condition === "Exists";
    }

    let actionValueDatalist = $state<string[]>([]);

    function isActionValueDisabled(actionType: RuleActionType) {
        return actionType === "DeleteTransaction" || actionType === "Clear";
    }

    function isActionFieldDisabled(actionType: RuleActionType) {
        return actionType === "DeleteTransaction";
    }

    let textValueDatalist = $state<string[]>([]);

    async function loadTextCompletions(
        field: RuleTransactionField,
        value?: string,
        condition?: RuleCondition,
    ) {
        const isAction = condition !== undefined;
        if (!value || (isAction && field === "Description")) {
            return;
        }

        const res = await apiClient.get<string[]>(`Completions/descriptions?value=${value}&condition=${condition}`);
        textValueDatalist = res.data;
    }

    function onFieldSelectChange(
        e: { currentTarget: HTMLSelectElement },
        actionOrTrigger: { field?: RuleTransactionField; value?: string; }
    ) {
        const field = actionOrTrigger.field;
        const target = e.currentTarget.value as RuleTransactionField;
        if (
            (field === "Source" || field === "Destination")
            && (target !== "Source" && target !== "Destination")
        ) {
            actionOrTrigger.value = accounts.find(a => a.value == actionOrTrigger.value)?.display ?? "";
        } else if (
            (target === "Source" || target === "Destination")
            && (field !== "Source" && field !== "Destination")
        ) {
            actionOrTrigger.value = accounts.find(a => a.display == actionOrTrigger.value)?.value;
        }

        actionOrTrigger.field = target;
    }
</script>


<div class="mb-8 flex justify-between">
    <div class="flex gap-4">
        <h1 class="h1">Rule</h1>
        <button onclick={save} class="btn variant-filled-primary">Save</button>
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
            <button
                class="btn variant-outline-primary"
                onclick={addTrigger}
            >New Trigger
            </button>
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
                            <button
                                class="btn variant-outline-error mr-4"
                                onclick={() => removeTrigger(idx)}
                            >Delete
                            </button>
                            <select
                                class="select w-fit"
                                value={trigger.field}
                                onchange={(e) => onFieldSelectChange(e, trigger)}
                            >
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
                            <RuleValueInput
                                disabled={isTriggerValueDisabled(trigger.condition)}
                                bind:value={trigger.value}
                                dataListId="text-value"
                                field={trigger.field}
                                accounts={accounts}
                                onLoadTextPredictions={(value: string) => loadTextCompletions(trigger.field, value)}
                                selectPopupName="trigger-value-{idx}"
                            />
                        </td>
                    </tr>
                {/each}
                </tbody>
            </table>
        </div>
        <datalist id="text-value">
            {#each textValueDatalist as option}
                <option>{option}</option>
            {/each}
        </datalist>

        <div class="mb-4 flex gap-4">
            <h2 class="h2"> Then rule will </h2>
            <button
                class="btn variant-outline-primary"
                onclick={addAction}
            >New Action
            </button>
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
                {#each rule.actions as action, idx}
                    <tr>
                        <td>
                            <button
                                class="btn variant-outline-error mr-4"
                                onclick={() => removeAction(idx)}
                            >Delete
                            </button>
                            <select class="select w-fit" bind:value={action.actionType}>
                                {#each ruleActionTypes as actionType}
                                    <option value={actionType}>{splitPascal(actionType)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <select
                                class="select w-fit"
                                value={action.field}
                                onchange={e => onFieldSelectChange(e, action)}
                                disabled={isActionFieldDisabled(action.actionType)}
                            >
                                {#each ruleTransactionFields as field}
                                    <option value={field}>{splitPascal(field)}</option>
                                {/each}
                            </select>
                        </td>
                        <td>
                            <RuleValueInput
                                disabled={isActionValueDisabled(action.actionType)}
                                bind:value={action.value}
                                dataListId="trigger-value"
                                field={action.field ?? 'Description'}
                                accounts={accounts}
                                onLoadTextPredictions={async () => console.log("hi there")}
                                selectPopupName="action-value-{idx}"
                            />
                        </td>
                    </tr>
                {/each}
                </tbody>
            </table>
        </div>
        <datalist id="action-value">
            {#each actionValueDatalist as option}
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
