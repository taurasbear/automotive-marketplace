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
import { cn } from "@/lib/utils";
import { ChevronDown } from "lucide-react";
import { useState } from "react";

type LocationComboboxProps = {
  selectedLocation?: string;
  onValueChange: (value: string) => void;
  className?: string;
};

const LocationCombobox = ({
  selectedLocation,
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
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="location-combobox"
            className={cn(
              className,
              "w-full justify-between bg-transparent font-normal",
            )}
          >
            <div className="grid grid-cols-1 justify-items-start">
              <label className="text-muted-foreground text-xs">Location</label>
              {selectedLocation ? (
                locations.find(
                  (location) => location.value === selectedLocation,
                )?.label
              ) : (
                <span className="text-muted-foreground truncate text-sm">
                  Any location
                </span>
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
                        newLocation == selectedLocation ? "any" : newLocation,
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
