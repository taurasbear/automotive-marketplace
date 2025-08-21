import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { ChevronDown } from "lucide-react";
import { useState } from "react";

type LocationComboboxProps = {
  selectedLocation?: string;
  onValueChange: (value: string) => void;
};

const LocationCombobox = ({
  selectedLocation,
  onValueChange,
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
            //aria-expanded={open}
            className="w-full justify-between font-normal"
          >
            {selectedLocation ? (
              locations.find((location) => location.value === selectedLocation)
                ?.label
            ) : (
              <p className="text-muted-foreground text-sm">Any location</p>
            )}
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
