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
import { getMunicipalitiesOptions } from "@/features/listingList/api/getMunicipalitiesOptions";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { ChevronDown } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

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
  const { t } = useTranslation("common");
  const [isPopoverOpen, setIsPopoverOpen] = useState<boolean>(false);
  const { data: municipalitiesResponse } = useQuery(getMunicipalitiesOptions());
  const municipalities = municipalitiesResponse?.data ?? [];

  const selectedName =
    value === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
      ? null
      : municipalities.find((m) => m.id === value)?.name;

  return (
    <div>
      <Popover open={isPopoverOpen} onOpenChange={setIsPopoverOpen}>
        <PopoverTrigger aria-label={t("aria.location")} asChild>
          <Button
            variant="outline"
            role="location-combobox"
            className={cn(
              "w-full justify-between bg-transparent font-normal",
              className,
            )}
          >
            <div className="grid grid-cols-1 justify-items-start">
              <span className="text-muted-foreground text-xs">
                {t("select.location")}
              </span>
              <span className="truncate text-sm">
                {selectedName ?? t("select.anyLocation")}
              </span>
            </div>
            <ChevronDown className="opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent>
          <Command>
            <CommandInput placeholder={t("select.searchLocation")} />
            <CommandList>
              <CommandEmpty>{t("select.noLocationFound")}</CommandEmpty>
              <CommandGroup>
                {municipalities.map((municipality) => (
                  <CommandItem
                    key={municipality.id}
                    value={municipality.name}
                    onSelect={() => {
                      onValueChange(
                        municipality.id === value
                          ? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
                          : municipality.id,
                      );
                      setIsPopoverOpen(false);
                    }}
                  >
                    {municipality.name}
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
