import { cn } from "@/lib/utils";
import { format } from "date-fns";
import { CalendarIcon } from "lucide-react";
import { ControllerRenderProps, FieldPath, FieldValues } from "react-hook-form";
import { Button } from "../ui/button";
import { Calendar } from "../ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";

type DatePickerProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = {
  field: ControllerRenderProps<TFieldValues, TName>;
};

const DatePicker = <
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>({
  field,
}: DatePickerProps<TFieldValues, TName>) => {
  const isValidDate = (value: unknown): value is Date => {
    return value instanceof Date && !isNaN(value.getTime());
  };

  const dateValue = isValidDate(field.value) ? field.value : undefined;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant={"outline"}
          className={cn(
            "w-[240px] pl-3 text-left font-normal",
            !dateValue && "text-muted-foreground",
          )}
        >
          {dateValue ? format(dateValue, "PPP") : <span>Pick a date</span>}
          <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="single"
          selected={dateValue}
          onSelect={field.onChange}
          disabled={(date: Date) =>
            date > new Date() || date < new Date("1900-01-01")
          }
          captionLayout="dropdown"
        />
      </PopoverContent>
    </Popover>
  );
};

export default DatePicker;
