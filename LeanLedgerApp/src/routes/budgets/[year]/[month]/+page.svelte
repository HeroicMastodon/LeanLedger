<script lang="ts">
    import {page} from "$app/stores";
    import {monthNameFrom, months} from "$lib/dateTools";

    const monthNumber = $derived(Number.parseInt($page.params.month));
    let monthName = $derived(monthNameFrom(monthNumber));
    let year = $derived($page.params.year);

</script>

<a href="/budgets/{year}/{monthNumber - 1}" class="btn">Previous</a>
<h1 class="h1">Budget for {monthName} {year}</h1>
<a href="/budgets/{year}/{monthNumber + 1}" class="btn">Next</a>
