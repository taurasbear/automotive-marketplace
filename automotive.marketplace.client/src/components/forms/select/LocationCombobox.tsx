import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { ChevronDown } from "lucide-react";
import { useState } from "react";

type LocationComboboxProps = {
  value: string;
  onValueChange: (value: string) => void;
  className?: string;
};

const LocationCombobox = ({
  value,
  onValueChange,
  className,
}: LocationComboboxProps) => {
  const locations = [
    { value: "kaunas", label: "Kaunas" },
    { value: "trakai", label: "Trakai" },
    { value: "vilnius", label: "Vilnius" },
  ];

  const [isPopoverOpen, setIsPopoverOpen] = useState<boolean>(false);

  return (
    <div>
      <Popover open={isPopoverOpen} onOpenChange={setIsPopoverOpen}>
        <PopoverTrigger aria-label="Location" asChild>
          <Button
            variant="outline"
            role="location-combobox"
            className={cn(
              "w-full justify-between bg-transparent font-normal",
              className,
            )}
          >
            <div className="grid grid-cols-1 justify-items-start">
              <span className="text-muted-foreground text-xs">Location</span>
              {value === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE ? (
                <span className="text-muted-foreground truncate text-sm">
                  {UI_CONSTANTS.SELECT.ANY_LOCATION.LABEL}
                </span>
              ) : (
                locations.find((location) => location.value === value)?.label
              )}
            </div>
            <ChevronDown className="opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent>
          <Command>
            <CommandInput placeholder="Search location" />
            <CommandList>
              <CommandEmpty>No location found.</CommandEmpty>
              <CommandGroup>
                {locations.map((location) => (
                  <CommandItem
                    key={location.value}
                    value={location.value.toString()}
                    onSelect={(newLocation) => {
                      onValueChange(
                        newLocation === value
                          ? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
                          : newLocation,
                      );
                      setIsPopoverOpen(false);
                    }}
                  >
                    {location.label}
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    </div>
  );
};

export default LocationCombobox;
