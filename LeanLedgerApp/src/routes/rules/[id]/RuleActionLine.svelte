<script lang="ts">
    import { splitPascal, type MaybePromise, type SelectOption } from "$lib";
    import LabeledSelect from "$lib/components/forms/LabeledSelect.svelte";
    import {
        isAccountField,
        ruleActionTypes,
        ruleTransactionFields,
        type RuleAction,
        type RuleActionType,
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
        onLoadTextPredictions,
    }: {
        action: RuleAction;
        onRemove: () => MaybePromise<void>;
        accounts: SelectOption<string>[];
        selectPopupId: string | number;
        onLoadTextPredictions: (value: string) => Promise<any>;
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
</script>

<div class="flex justify-between items-center">
    <div class="flex justify-start items-center gap-4">
        <button class="btn text-error-400 mr-4 p-0" onclick={onRemove}>
            <Fa icon={faTrashCan} />
        </button>
        <LabeledSelect
            class="w-fit"
            label="Action"
            bind:value={action.actionType}
            options={ruleActionTypes.map((t) => ({
                value: t,
                display: splitPascal(t),
            }))}
        />
    </div>
    <LabeledSelect
        class="w-fit"
        label="Field"
        value={action.field}
        disabled={isActionFieldDisabled(action.actionType)}
        onchange={(e) => onFieldSelectChange(e, action)}
        options={validFieldsForAction(action.actionType).map((t) => ({
            value: t,
            display: splitPascal(t),
        }))}
    />
    <RuleValueInput
        label="Value"
        disabled={isActionValueDisabled(action.actionType)}
        bind:value={action.value}
        dataListId="text-value"
        field={action.field ?? "Description"}
        {accounts}
        {onLoadTextPredictions}
        selectPopupName="action-value-{selectPopupId}"
    />
</div>
