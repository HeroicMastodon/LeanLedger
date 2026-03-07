<script lang="ts">
    import TransactionTable from "$lib/transactions/TransactionTable.svelte";
    import {
        defaultTransaction,
        type EditableTransaction,
        type Transaction,
    } from "$lib/transactions";
    import { apiClient } from "$lib/apiClient";
    import { ProgressBar } from "@skeletonlabs/skeleton";
    import TransactionForm from "$lib/transactions/TransactionForm.svelte";
    import type { AxiosError } from "axios";
    import FormButton from "$lib/components/dialog/FormButton.svelte";
    import Alert from "$lib/components/Alert.svelte";
    import { monthManager } from "$lib/selectedMonth.svelte";
    import LabeledInput from "$lib/components/forms/LabeledInput.svelte";
    import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
    import { debounce } from "$lib/rules";
    import { pushState } from "$app/navigation";

    let transactions: Transaction[] = $state([]);

    async function load() {
        const response = await apiClient.get<Transaction[]>(
            `transactions?${monthManager.params}`,
        );
        transactions = response.data;
    }

    let transaction = $state(defaultTransaction());
    let error: string | undefined = $state();

    async function saveTransaction() {
        try {
            error = undefined;
            const resp = await apiClient.post<EditableTransaction>(
                "transactions",
                transaction,
            );
            transaction = defaultTransaction();
            await load();
        } catch (e) {
            const err = e as AxiosError<{ detail: string }>;
            console.dir(err.response?.data.detail);
            error = err.response?.data.detail;
            return false;
        }

        return true;
    }
    // initialize search query from URL params
    const params = new URLSearchParams(window.location.search);
    const initialSearch = params.get("search") ?? "";
    let search = $state(initialSearch);
    const searchLower = $derived(search.toLowerCase());
    const filteredTransactions = $derived(
        transactions.filter(
            (t) =>
                t.description.toLowerCase().includes(searchLower) ||
                t.sourceAccount?.name.toLowerCase().includes(searchLower) ||
                t.destinationAccount?.name
                    .toLowerCase()
                    .includes(searchLower) ||
                t.category?.toLowerCase().includes(searchLower) ||
                (!t.category && "none".includes(searchLower)),
        ),
    );

    const onSearchChange = debounce(
        (e: Event & { currentTarget: EventTarget & HTMLInputElement }) => {
            console.log("Updating search query to", search);
            updateSearchQuery(search);
        },
    );

    function updateSearchQuery(searchValue: string) {
        const params = new URLSearchParams(window.location.search);
        if (searchValue) {
            params.set("search", searchValue);
        } else {
            params.delete("search");
        }

        pushState(`${window.location.pathname}?${params.toString()}`, {});
    }
</script>

<div class="mb-8 flex justify-start items-center gap-4 md:gap-8 flex-wrap">
    <h1 class="h1">Transactions</h1>
    <FormButton
        class="btn-icon-sm p-2 variant-outline-primary text-primary-500"
        text="New Transaction"
        bind:error
        onConfirm={saveTransaction}
        confirmText="Create"
        icon={faPlus}
    >
        <TransactionForm bind:transaction />
    </FormButton>
    <LabeledInput
        type="text"
        bind:value={search}
        placeholder="Search..."
        onkeyup={onSearchChange}
    />
</div>
<TransactionTable transactions={filteredTransactions} />
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:catch err}
    <Alert show>
        <h3 class="h3">Something went wrong</h3>
        <p>We couldn't load your transactions. Please try again</p>
        <p>{!!err ? err : ""}</p>
    </Alert>
{/await}
