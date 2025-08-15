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

const LocationCombobox = () => {
  const locations = [
    { value: "kaunas", label: "Kaunas" },
    { value: "trakai", label: "Trakai" },
    { value: "vilnius", label: "Vilnius" },
  ];

  const [currentLocation, setCurrentLocation] = useState<string>("");
  const [open, setOpen] = useState<boolean>(false);

  return (
    <div>
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="location-combobox"
            //aria-expanded={open}
            className="w-full justify-between font-normal"
          >
            {currentLocation ? (
              locations.find((location) => location.value === currentLocation)
                ?.label
            ) : (
              <p className="text-muted-foreground text-sm">Any</p>
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
                      setCurrentLocation(
                        newLocation == currentLocation ? "" : newLocation,
                      );
                      setOpen(false);
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
