import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useState } from 'react';

type MakeOfferModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: 'offer' | 'counter';
  listingPrice: number;
  conversationId?: string;
  offerId?: string;
  initialAmount?: number;
  onSubmit: (amount: number) => void;
};

const MakeOfferModal = ({
  open,
  onOpenChange,
  mode,
  listingPrice,
  initialAmount,
  onSubmit,
}: MakeOfferModalProps) => {
  const [rawValue, setRawValue] = useState(
    initialAmount !== undefined ? String(Math.round(initialAmount)) : '',
  );

  const amount = parseFloat(rawValue);
  const minAmount = listingPrice / 3;
  const isValidNumber = !isNaN(amount) && amount > 0;
  const isTooLow = isValidNumber && amount < minAmount;
  const isTooHigh = isValidNumber && amount > listingPrice;
  const isValid = isValidNumber && !isTooLow && !isTooHigh;

  const percentageOff =
    isValidNumber && amount <= listingPrice
      ? Math.round(((listingPrice - amount) / listingPrice) * 100)
      : null;

  const handleSubmit = () => {
    if (!isValid) return;
    onSubmit(amount);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{mode === 'counter' ? 'Counter Offer' : 'Make an Offer'}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="bg-muted flex justify-between rounded-lg px-4 py-3 text-sm">
            <div>
              <p className="text-muted-foreground text-xs">Listed price</p>
              <p className="font-semibold">€{listingPrice.toLocaleString()}</p>
            </div>
            {percentageOff !== null && percentageOff > 0 && (
              <div className="text-right">
                <p className="text-muted-foreground text-xs">Discount</p>
                <p className="text-destructive font-semibold">−{percentageOff}%</p>
              </div>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="offer-amount">Your offer (€)</Label>
            <Input
              id="offer-amount"
              type="number"
              min={Math.ceil(minAmount)}
              max={listingPrice}
              value={rawValue}
              onChange={(e) => setRawValue(e.target.value)}
              placeholder={`${Math.ceil(minAmount)} – ${listingPrice}`}
            />
            {isTooLow && (
              <p className="text-destructive text-xs">
                Minimum offer is €{Math.ceil(minAmount).toLocaleString()} (⅓ of asking price).
              </p>
            )}
            {isTooHigh && (
              <p className="text-destructive text-xs">
                Offer cannot exceed the listing price of €{listingPrice.toLocaleString()}.
              </p>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!isValid}>
              {mode === 'counter' ? 'Send Counter' : 'Send Offer'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default MakeOfferModal;
