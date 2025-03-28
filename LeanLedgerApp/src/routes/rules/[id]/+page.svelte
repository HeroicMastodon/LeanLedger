<script lang="ts">
    import {
        defaultRule,
        defaultRuleAction,
        defaultRuleTrigger,
        type Rule,
        type RuleAction,
        type RuleActionType,
        ruleActionTypes,
        type RuleCondition,
        type RuleTransactionField,
        ruleTransactionFields,
        type RuleTrigger
    } from "$lib/rules";
    import Alert from "$lib/components/Alert.svelte";
    import {ProgressBar} from "@skeletonlabs/skeleton";
    import {apiClient} from "$lib/apiClient";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import {type SelectOption, splitPascal,} from "$lib";
    import RuleValueInput from "$lib/rules/RuleValueInput.svelte";
    import {loadAccountOptions, type Transaction} from "$lib/transactions";
    import {goto} from "$app/navigation"
    import DeleteConfirmationButton from "$lib/components/dialog/DeleteConfirmationButton.svelte";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import RunRuleButton from "$lib/rules/RunRuleButton.svelte";
    import PredictiveSelect from "$lib/components/forms/PredictiveSelect.svelte";
    import {firstOfThisYear, todaysDateString} from "$lib/dateTools";
    import {page} from "$app/stores";
    import {Fa} from "svelte-fa";
    import {faSave} from "@fortawesome/free-solid-svg-icons/faSave";
    import {faSearchDollar} from "@fortawesome/free-solid-svg-icons/faSearchDollar";
    import {faPlus} from "@fortawesome/free-solid-svg-icons/faPlus";
    import {faTrashCan} from "@fortawesome/free-solid-svg-icons/faTrashCan";

    let id = $page.params.id;
    let rule = $state<Rule>(defaultRule());
    let loading = $state(load());
    let accounts = $state<SelectOption<string>[]>([]);
    let ruleGroups = $state<SelectOption<string>[]>([]);

    async function load() {
        const ruleResponse = await apiClient.get<Rule>(`Rules/${id}`);
        rule = ruleResponse.data;

        accounts = await loadAccountOptions();
        const ruleGroupRes = await apiClient.get<string[]>("completions/rule-groups");
        ruleGroups = ruleGroupRes.data.map(name => ({value: name, display: name}));
    }

    async function save() {
        rule.triggers.forEach(trigger => {
            if (typeof (trigger.value as unknown) === "number") {
                trigger.value = (trigger.value as unknown as number).toString()
            }
        })
        rule.actions.forEach(action => {
            if (typeof (action.value as unknown) === "number") {
                action.value = (action.value as unknown as number).toString()
            }
        })

        const res = await apiClient.put<Rule>(`Rules/${id}`, rule);
        rule = res.data;
    }

    async function deleteRule() {
        try {
            const resp = await apiClient.delete(`rules/${id}`);
            await goto("/rules");

            return true;
        } catch (e) {
            return false;
        }
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
        const isAction = condition === undefined;
        if (!value || (isAction && field === "Description")) {
            return;
        }

        if (field === "Description") {
            const res = await apiClient.get<string[]>(`Completions/descriptions?value=${value}&condition=${condition}`);
            textValueDatalist = res.data;
        } else if (field === "Category") {
            const res = await apiClient.get<string[]>(`Completions/categories?value=${value}&condition=${condition}`);
            textValueDatalist = res.data;
        }
    }

    function onFieldSelectChange(
        e: { currentTarget: HTMLSelectElement },
        actionOrTrigger: RuleAction | RuleTrigger
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

    function validConditionsForField(field: RuleTransactionField): RuleCondition[] {
        switch (field) {
            case "Description":
                return [
                    "Exists",
                    "IsExactly",
                    "Contains",
                    "EndsWith",
                    "StartsWith",
                ];
            case "Category":
                return [
                    "IsExactly",
                    "Contains",
                    "EndsWith",
                    "StartsWith",
                ];
            case "Date":
            case "Amount":
                return [
                    "IsExactly",
                    "GreaterThan",
                    "LessThan",
                ];
            case "Type":
                return [
                    "IsExactly"
                ];
            case "Source":
            case "Destination":
                return [
                    "IsExactly",
                    "Exists"
                ];
            default:
                return [];
        }
    }

    function validFieldsForAction(actionType: RuleActionType): readonly RuleTransactionField[] {
        switch (actionType) {
            case "Append":
                return [
                    "Category",
                    "Description",
                ];
            case "Set":
                return ruleTransactionFields;
            case "Clear":
                return [
                    "Source",
                    "Destination",
                    "Category",
                ];
            case "DeleteTransaction":
                return [];
            default:
                return ruleTransactionFields;
        }
    }

    let matchingTransactions: Promise<Transaction[]> = $state(Promise.resolve([]))
    let startDate = $state(firstOfThisYear());
    let endDate = $state(todaysDateString());

    function findMatchingTransactions() {
        matchingTransactions = apiClient
            .get<Transaction[]>(`rules/${id}/matching?start=${startDate}&end=${endDate}&limit=5`)
            .then(res => res.data);

        return false;
    }

    let transactionCount: Promise<number | undefined> = $state(Promise.resolve(undefined));

    function runRule(startDate: string, endDate: string) {
        transactionCount = apiClient
            .post<{ count: number }>(`rules/${id}/run`, {startDate, endDate})
            .then(res => {
                return res.data.count;
            });
    }
</script>


<div class="mb-8 flex justify-between">
    <div class="flex gap-4 items-center flex-wrap">
        <h1 class="h1">Rule</h1>
        <button onclick={save} class="btn text-primary-500 p-2">
            <Fa icon={faSave} />
        </button>
        <FormButton
            class="text-secondary-500"
            text="Find Matching Transactions"
            confirmText="Find"
            onConfirm={findMatchingTransactions}
            icon={faSearchDollar}
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
            {#await matchingTransactions}
                <TransactionTable transactions={[]} />
                <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
            {:then transactions}
                <div class="max-h-80 overflow-y-auto">
                    <TransactionTable transactions={transactions} />
                </div>
            {:catch err}
                <Alert class="variant-filled-error"><p>{err}</p></Alert>
            {/await}
        </FormButton>
        <RunRuleButton
            text="Run Rule"
            countPromise={transactionCount}
            run={runRule}
        />
    </div>
    <DeleteConfirmationButton onDelete={deleteRule} />
</div>

{#await loading}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <div class="flex gap-8 items-end w-fit mb-8 flex-wrap">
        <LabeledInput
            label="Name"
            bind:value={rule.name}
            type="text"
        />
        <PredictiveSelect
            label="Rule Group"
            optional
            popupTargetName="rule-group-items"
            bind:value={rule.ruleGroupName}
            options={ruleGroups}
        />
        <label class="md:col-span-1 flex space-x-2 items-center">
            <input type="checkbox" class="checkbox mt-2 mb-2" bind:checked={rule.isStrict} />
            <span>Is Strict?</span>
        </label>
    </div>

    <div class="mb-4 flex gap-4">
        <h2 class="h2"> Rule triggers when </h2>
        <button
            class="btn variant-outline-primary text-primary-500 p-2"
            onclick={addTrigger}
        >
            <Fa icon={faPlus} />
        </button>
    </div>
    <div class="table-container mb-8">
        <table class="table">
            <thead>
            <tr>
                <th>Field</th>
                <th>Not</th>
                <th>Condition</th>
                <th>Value</th>
            </tr>
            </thead>
            <tbody>
            {#each rule.triggers as trigger, idx}
                <tr>
                    <td>
                        <button
                            class="btn text-error-500 mr-4 p-0"
                            onclick={() => removeTrigger(idx)}
                        >
                            <Fa icon={faTrashCan} />
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
                            {#each validConditionsForField(trigger.field) as condition}
                                <option value={condition}>{splitPascal(condition)}</option>
                            {/each}
                        </select>
                    </td>
                    <td class="min-w-64">
                        <RuleValueInput
                            disabled={isTriggerValueDisabled(trigger.condition)}
                            bind:value={trigger.value}
                            dataListId="text-value"
                            field={trigger.field}
                            accounts={accounts}
                            onLoadTextPredictions={value => loadTextCompletions(trigger.field, value, trigger.condition)}
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
            class="btn variant-outline-primary text-primary-500 p-2"
            onclick={addAction}
        >
            <Fa icon={faPlus} />
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
                            class="btn text-error-500 mr-4 p-0"
                            onclick={() => removeAction(idx)}
                        >
                            <Fa icon={faTrashCan} />
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
                            {#each validFieldsForAction(action.actionType) as field}
                                <option value={field}>{splitPascal(field)}</option>
                            {/each}
                        </select>
                    </td>
                    <td class="min-w-64">
                        <RuleValueInput
                            disabled={isActionValueDisabled(action.actionType)}
                            bind:value={action.value}
                            dataListId="text-value"
                            field={action.field ?? 'Description'}
                            accounts={accounts}
                            onLoadTextPredictions={value => loadTextCompletions(action.field ?? "Category", value, "Contains")}
                            selectPopupName="action-value-{idx}"
                        />
                    </td>
                </tr>
            {/each}
            </tbody>
        </table>
    </div>
{:catch err}
    <Alert class="variant-filled-error">
        <p>Could not load rule:</p>
        <p>{err}</p>
    </Alert>
{/await}
