<script lang="ts">
    import { splitPascal, type MaybePromise, type SelectOption } from "$lib";
    import { apiClient } from "$lib/apiClient";
    import {
        isAccountField,
        ruleActionTypes,
        ruleTransactionFields,
        type RuleAction,
        type RuleActionType,
        type RuleCondition,
        type RuleTransactionField,
    } from "$lib/rules";
    import RuleValueInput from "$lib/rules/RuleValueInput.svelte";
    import { faTrashCan } from "@fortawesome/free-solid-svg-icons/faTrashCan";
    import Fa from "svelte-fa";

    let {
        action = $bindable(),
        onRemove,
        accounts,
        selectPopupId,
    }: {
        action: RuleAction;
        onRemove: () => MaybePromise<void>;
        accounts: SelectOption<string>[];
        selectPopupId: string | number;
    } = $props();

    function validFieldsForAction(
        actionType: RuleActionType,
    ): readonly RuleTransactionField[] {
        switch (actionType) {
            case "Append":
                return ["Category", "Description"];
            case "Set":
                return ruleTransactionFields;
            case "Clear":
                return ["Source", "Destination", "Category"];
            case "DeleteTransaction":
                return [];
            default:
                return ruleTransactionFields;
        }
    }

    function onFieldSelectChange(
        e: { currentTarget: HTMLSelectElement },
        actionOrTrigger: { field?: RuleTransactionField; value?: string },
    ) {
        const currentField = actionOrTrigger.field;
        const targetField = e.currentTarget.value as RuleTransactionField;

        if (isAccountField(currentField) && !isAccountField(targetField)) {
            actionOrTrigger.value =
                accounts.find((a) => a.value == actionOrTrigger.value)
                    ?.display ?? "";
        } else if (
            isAccountField(targetField) &&
            !isAccountField(currentField)
        ) {
            actionOrTrigger.value = accounts.find(
                (a) => a.display == actionOrTrigger.value,
            )?.value;
        }

        actionOrTrigger.field = targetField;
    }

    function isActionFieldDisabled(actionType: RuleActionType) {
        return actionType === "DeleteTransaction";
    }

    function isActionValueDisabled(actionType: RuleActionType) {
        return actionType === "DeleteTransaction" || actionType === "Clear";
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
            const res = await apiClient.get<string[]>(
                `Completions/descriptions?value=${value}&condition=${condition}`,
            );
            textValueDatalist = res.data;
        } else if (field === "Category") {
            const res = await apiClient.get<string[]>(
                `Completions/categories?value=${value}&condition=${condition}`,
            );
            textValueDatalist = res.data;
        }
    }
</script>

<tr>
    <td>
        <button class="btn text-error-400 mr-4 p-0" onclick={onRemove}>
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
            onchange={(e) => onFieldSelectChange(e, action)}
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
            field={action.field ?? "Description"}
            {accounts}
            onLoadTextPredictions={(value) =>
                loadTextCompletions(
                    action.field ?? "Category",
                    value,
                    "Contains",
                )}
            selectPopupName="action-value-{selectPopupId}"
        />
    </td>
</tr>
