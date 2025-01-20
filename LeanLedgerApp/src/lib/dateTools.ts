export function todaysDateString() {
    return new Date().toLocaleDateString('en-CA', {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric'
    })
}

export function firstOfThisYear() {
    const now = new Date();
    const year = now.getFullYear();
    const firstOfYear = new Date(year, 0, 1); // January is month 0
    return firstOfYear.toISOString().split('T')[0]; // Extracts yyyy-MM-dd
}

export type Month = {
    name: string;
    number: string | number;
    year: string | number;
}

export const months = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
] as const;
export function monthNameFrom(monthNumber: string | number) {
    return months[Number(monthNumber) - 1];
}

export function monthFromNumber(monthNumber: string | number, year: string | number): Month {
    return {
        number: monthNumber,
        year,
        name: monthNameFrom(monthNumber)
    }
}

export function nextMonth(curr: Month) {
    if (curr.number == 12) {
        return monthFromNumber(1, Number(curr.year) + 1);
    }

    return monthFromNumber(Number(curr.number) + 1, curr.year);
}

export function lastMonth(curr: Month) {
    if (curr.number == 1) {
        return monthFromNumber(12, Number(curr.year) - 1);
    }

    return monthFromNumber(Number(curr.number) - 1, curr.year);
}
