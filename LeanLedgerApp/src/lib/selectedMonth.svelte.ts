import {lastMonth as getLastMonth, type Month, monthFromNumber, nextMonth as getNextMonth} from "$lib/dateTools";

let today = new Date();

class MonthManager {
    selectedMonth = $state<Month>(monthFromNumber(today.getMonth() + 1, today.getFullYear()));
    nextMonth = $derived(getNextMonth(this.selectedMonth))
    lastMonth = $derived(getLastMonth(this.selectedMonth))

    increment() {
        this.selectedMonth = this.nextMonth;
    }

    decrement() {
        this.selectedMonth = this.lastMonth;
    }

    selectMonth(month: number | string, year: number | string) {
        this.selectedMonth = monthFromNumber(month, year);
    }

    selectFromQuery(params: URLSearchParams) {
        const month = params.get("month");
        const year = params.get("year");
        if (month && year) {
            this.selectMonth(month, year);
        }
    }
}

export const monthManager = new MonthManager();
