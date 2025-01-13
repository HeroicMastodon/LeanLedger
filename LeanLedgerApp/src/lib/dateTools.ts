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
